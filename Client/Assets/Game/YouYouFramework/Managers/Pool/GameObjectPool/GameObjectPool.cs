using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;


namespace YouYouFramework
{
    /// <summary>
    /// 游戏物体对象池
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// 游戏物体对象池字典
        /// </summary>
        public Dictionary<SpawnPoolId, SpawnPoolEntity> spawnPoolDic = new Dictionary<SpawnPoolId, SpawnPoolEntity>();

        /// <summary>
        /// Key==GameObject的InstanceId
        /// </summary>
        private Dictionary<int, PrefabPool> instanceIdPoolIdDic = new Dictionary<int, PrefabPool>();

        /// <summary>
        /// Key==Prefab的InstanceId
        /// </summary>
        private Dictionary<int, AssetReferenceEntity> prefabAssetDic = new Dictionary<int, AssetReferenceEntity>();

        public GameObject YouYouObjPool { get; private set; }


        public GameObjectPool()
        {
            //对象池物体克隆请求
            InstanceHandler.InstantiateDelegates += InstantiateDelegate;
            InstanceHandler.DestroyDelegates += DestroyDelegate;

            //初始化跨场景不销毁的对象池
            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                SpawnPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

                if (entity.IsGlobal)
                {
                    //创建对象池
                    SpawnPool pool = new GameObject(entity.PoolName + "Pool").AddComponent<SpawnPool>();
                    pool.transform.SetParent(GameEntry.Instance.transform);
                    pool.transform.localPosition = Vector3.zero;
                    pool.transform.localPosition = Vector3.zero;
                    entity.Pool = pool;
                    spawnPoolDic[entity.PoolId] = entity;
                }
            }
        }
        private GameObject InstantiateDelegate(PrefabPool prefabPool)
        {
            GameObject prefab = prefabPool.prefab;
            GameObject inst = Object.Instantiate(prefab);
            inst.transform.SetParent(prefabPool.spawnPool.transform, false);

            //从实例字典上 映射实例
            int instanceID = inst.GetInstanceID();
            instanceIdPoolIdDic[instanceID] = prefabPool;

            //让资源的引用计数+1
            if (prefabPool.TotalCount == 1)
            {
                if (prefabAssetDic.TryGetValue(prefab.GetInstanceID(), out AssetReferenceEntity referenceEntity))
                {
                    referenceEntity.ReferenceAdd();
                }
            }

            return inst;
        }
        private void DestroyDelegate(GameObject inst, PrefabPool prefabPool)
        {
            int instanceID = inst.GetInstanceID();

            //从实例字典上 移除实例
            instanceIdPoolIdDic.Remove(instanceID);

            //让资源的引用计数-1
            if (prefabPool.TotalCount == 0)
            {
                if (prefabAssetDic.TryGetValue(instanceID, out AssetReferenceEntity referenceEntity))
                {
                    referenceEntity.ReferenceRemove();
                }
            }

            Object.Destroy(inst);
        }

        /// <summary>
        /// 初始化跨场景销毁的对象池
        /// </summary>
        internal void InitScenePool()
        {
            if (YouYouObjPool == null) YouYouObjPool = new GameObject("YouYouObjPool");

            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                SpawnPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

                if (!entity.IsGlobal)
                {
                    //创建对象池
                    SpawnPool pool = new GameObject(entity.PoolName + "Pool").AddComponent<SpawnPool>();
                    pool.transform.SetParent(YouYouObjPool.transform);
                    pool.transform.localPosition = Vector3.zero;
                    pool.transform.localPosition = Vector3.zero;
                    entity.Pool = pool;
                    spawnPoolDic[entity.PoolId] = entity;
                }
            }
        }

        /// <summary>
        /// 预加载对象池
        /// </summary>
        public void PreloadObj(GameObject prefab, SpawnPoolId poolId, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 10, int cullMaxPerPass = 0)
        {
            //拿到分类池
            spawnPoolDic.TryGetValue(poolId, out SpawnPoolEntity gameObjectPoolEntity);
            if (gameObjectPoolEntity == null)
            {
                GameEntry.LogError(LogCategory.Pool, "gameObjectPoolEntity==null");
                return;
            }

            //对象池配置
            PrefabPool prefabPool = new PrefabPool(prefab, cullDespawned, cullAbove, cullDelay, cullMaxPerPass);
            gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);
            prefabPool.PreloadPool();
        }

        #region Spawn 从对象池中获取对象
        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        public GameObject Spawn(GameObject prefab, SpawnPoolId poolId = SpawnPoolId.Common)
        {
            if (prefab == null)
            {
                GameEntry.LogError(LogCategory.Pool, "prefab==null");
                return null;
            }

            //拿到分类池
            spawnPoolDic.TryGetValue(poolId, out SpawnPoolEntity gameObjectPoolEntity);
            if (gameObjectPoolEntity == null)
            {
                GameEntry.LogError(LogCategory.Pool, "gameObjectPoolEntity==null");
                return null;
            }

            PrefabPool prefabPool = gameObjectPoolEntity.Pool.GetPrefabPool(prefab);
            if (prefabPool == null)
            {
                //对象池配置
                prefabPool = new PrefabPool(prefab);
                gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);
            }

            //拿到一个实例
            GameObject inst = prefabPool.SpawnInstance();
            return inst;
        }
        public GameObject Spawn(string prefabFullPath, SpawnPoolId poolId = SpawnPoolId.Common)
        {
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(prefabFullPath);
            GameObject prefab = referenceEntity.Target as GameObject;
            prefabAssetDic[prefab.GetInstanceID()] = referenceEntity;
            return Spawn(prefab, poolId);
        }
        public async UniTask<GameObject> SpawnAsync(string prefabFullPath, SpawnPoolId poolId = SpawnPoolId.Common)
        {
            AssetReferenceEntity referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(prefabFullPath);
            GameObject prefab = referenceEntity.Target as GameObject;
            prefabAssetDic[prefab.GetInstanceID()] = referenceEntity;
            return Spawn(prefab, poolId);
        }
        #endregion

        #region Despawn 对象回池
        /// <summary>
        /// 对象回池
        /// </summary>
        public void Despawn(GameObject inst)
        {
            if (inst == null) return;

            int instanceID = inst.GetInstanceID();
            if (instanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                if (prefabPool.spawnPool == null) return;

                inst.transform.SetParent(prefabPool.spawnPool.transform, false);
                inst.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                prefabPool.DespawnInstance(inst);
            }
        }

        /// <summary>
        /// 全部对象回池
        /// </summary>
        public void DespawnAll()
        {
            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                SpawnPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

                if (entity.Pool != null)
                {
                    foreach (Transform item in entity.Pool.transform)
                    {
                        if (item.gameObject.activeInHierarchy)
                        {
                            Despawn(item.gameObject);
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 直接销毁对象
        /// </summary>
        public void Destroy(GameObject inst)
        {
            int instanceID = inst.GetInstanceID();
            if (instanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                prefabPool.Destroy(inst);
            }
            else
            {
                GameEntry.LogError(LogCategory.Pool, "该对象不在池内, inst==" + inst);
            }
        }

    }
}
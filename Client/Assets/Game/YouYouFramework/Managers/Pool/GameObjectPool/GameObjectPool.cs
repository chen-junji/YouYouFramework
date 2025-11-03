using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;


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
        public Dictionary<SpawnPoolId, SpawnPoolEntity> spawnPoolDic = new();

        /// <summary>
        /// Key==GameObject的InstanceId
        /// </summary>
        private Dictionary<int, PrefabPool> instanceIdPoolIdDic = new();

        /// <summary>
        /// Key==Prefab的InstanceId
        /// </summary>
        private Dictionary<int, AsyncOperationHandle> prefabAssetDic = new();

        public GameObject YouYouObjPool { get; private set; }


        public GameObjectPool()
        {
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
            GameObject inst = Object.Instantiate(prefabPool.prefab, prefabPool.Root.transform, false);

            //从实例字典上 映射实例
            instanceIdPoolIdDic[inst.GetInstanceID()] = prefabPool;

            return inst;
        }
        private void DestroyDelegate(GameObject inst, PrefabPool prefabPool)
        {
            //从实例字典上 移除实例
            instanceIdPoolIdDic.Remove(inst.GetInstanceID());

            Object.Destroy(inst);
        }
        private void DestructDelegate(PrefabPool prefabPool)
        {
            if (prefabAssetDic.TryGetValue(prefabPool.prefab.GetInstanceID(), out var referenceEntity))
            {
                referenceEntity.Release();
            }
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
        public void PreloadObj(GameObject prefab, SpawnPoolId poolId, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 60, int cullMaxPerPass = 30)
        {
            //拿到分类池
            spawnPoolDic.TryGetValue(poolId, out SpawnPoolEntity gameObjectPoolEntity);
            if (gameObjectPoolEntity == null)
            {
                GameEntry.LogError(LogCategory.Pool, "gameObjectPoolEntity==null");
                return;
            }

            //对象池配置
            PrefabPool prefabPool = new(prefab, cullDespawned, cullAbove, cullDelay, cullMaxPerPass);
            gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);
            prefabPool.PreloadPool();
            prefabPool.CreateFunc = () => InstantiateDelegate(prefabPool);
            prefabPool.ActionOnDestroy = (inst) => DestroyDelegate(inst, prefabPool);
            prefabPool.ActionOnDestruct = () => DestructDelegate(prefabPool);
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
                prefabPool = new(prefab);
                gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);
                prefabPool.CreateFunc = () => InstantiateDelegate(prefabPool);
                prefabPool.ActionOnDestroy = (inst) => DestroyDelegate(inst, prefabPool);
                prefabPool.ActionOnDestruct = () => DestructDelegate(prefabPool);
            }

            //拿到一个实例
            GameObject inst = prefabPool.SpawnInstance();
            return inst;
        }
        public async UniTask<GameObject> Spawn(string prefabFullPath, SpawnPoolId poolId = SpawnPoolId.Common)
        {
            var referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(prefabFullPath);
            GameObject prefab = referenceEntity.Result as GameObject;

            if (prefabAssetDic.ContainsKey(prefab.GetInstanceID()))
            {
                referenceEntity.Release();
            }
            else
            {
                prefabAssetDic[prefab.GetInstanceID()] = referenceEntity;
            }
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
                if (prefabPool.Root == null) return;

                inst.transform.SetParent(prefabPool.Root.transform, false);
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
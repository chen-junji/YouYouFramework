//===================================================
using YouYouFramework;
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
        public Dictionary<byte, GameObjectPoolEntity> spawnPoolDic = new Dictionary<byte, GameObjectPoolEntity>();

        /// <summary>
        /// Key==GameObject的InstanceId
        /// </summary>
        private Dictionary<int, PrefabPool> instanceIdPoolIdDic = new Dictionary<int, PrefabPool>();

        /// <summary>
        /// Key==Prefab的InstanceId
        /// </summary>
        private Dictionary<int, AssetReferenceEntity> prefabAssetDic = new Dictionary<int, AssetReferenceEntity>();

        public GameObject YouYouObjPool { get; private set; }


        /// <summary>
        /// 初始化跨场景不销毁的对象池
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                GameObjectPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

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
        /// <summary>
        /// 初始化跨场景销毁的对象池
        /// </summary>
        internal void InitScenePool()
        {
            if (YouYouObjPool == null) YouYouObjPool = new GameObject("YouYouObjPool");

            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                GameObjectPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

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

        public GameObjectPool()
        {
            //对象池物体克隆请求
            InstanceHandler.InstantiateDelegates += InstantiateDelegate;
            InstanceHandler.DestroyDelegates += DestroyDelegate;
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
        /// 预加载对象池
        /// </summary>
        public void PreloadObj(GameObject prefab, int count)
        {
            List<GameObject> preloadList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject poolObj = Spawn(prefab);
                preloadList.Add(poolObj);
            }
            for (int i = 0; i < count; i++)
            {
                Despawn(preloadList[i]);
            }
        }

        #region Spawn 从对象池中获取对象
        public async UniTask<GameObject> SpawnAsync(string prefabName)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName);
            return await SpawnAsync(sys_Prefab);
        }
        public async UniTask<GameObject> SpawnAsync(Sys_PrefabEntity entity)
        {
            AssetReferenceEntity referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(entity.AssetFullPath);
            GameObject prefab = referenceEntity.Target as GameObject;
            if (prefab == null)
            {
                GameEntry.LogError(LogCategory.Loader, "找不到Prefab, AssetFullName==" + entity.AssetFullPath);
                return null;
            }
            prefabAssetDic[prefab.GetInstanceID()] = referenceEntity;

            return Spawn(prefab, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        public GameObject Spawn(string prefabName)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName);
            return Spawn(sys_Prefab);
        }
        public GameObject Spawn(Sys_PrefabEntity entity)
        {
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(entity.AssetFullPath);
            GameObject prefab = referenceEntity.Target as GameObject;
            if (prefab == null)
            {
                GameEntry.LogError(LogCategory.Loader, "找不到Prefab, AssetFullPath==" + entity.AssetFullPath);
                return null;
            }
            prefabAssetDic[prefab.GetInstanceID()] = referenceEntity;

            return Spawn(prefab, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }
        public GameObject Spawn(GameObject prefab, byte poolId = 1, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 10, int cullMaxPerPass = 0)
        {
            if (prefab == null)
            {
                return null;
            }
            //拿到对象池
            GameObjectPoolEntity gameObjectPoolEntity = spawnPoolDic[poolId];

            PrefabPool prefabPool = gameObjectPoolEntity.Pool.GetPrefabPool(prefab);
            if (prefabPool == null)
            {
                //对象池配置
                prefabPool = new PrefabPool(prefab);
                prefabPool.cullDespawned = cullDespawned;
                prefabPool.cullAbove = cullAbove;
                prefabPool.cullDelay = cullDelay;
                prefabPool.cullMaxPerPass = cullMaxPerPass;

                gameObjectPoolEntity.Pool.CreatePrefabPool(prefabPool);
            }

            //拿到一个实例
            bool isNewInstance = false;
            GameObject inst = prefabPool.SpawnInstance(ref isNewInstance);
            return inst;
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

            AutoDespawnHandle handle = inst.GetComponent<AutoDespawnHandle>();
            if (handle != null) handle.StopTime();
        }

        /// <summary>
        /// 全部对象回池
        /// </summary>
        public void DespawnAll()
        {
            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                GameObjectPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

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
        /// 直接释放对象
        /// </summary>
        public void Release(GameObject inst)
        {
            int instanceID = inst.GetInstanceID();
            if (instanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                prefabPool.Release(inst);
            }
            else
            {
                GameEntry.LogError(LogCategory.Pool, "该对象不在池内, inst==" + inst);
            }
        }

    }
}
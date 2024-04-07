//===================================================
using YouYouFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using static UnityEditor.PlayerSettings;

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
        public Dictionary<byte, GameObjectPoolEntity> m_SpawnPoolDic;

        /// <summary>
        /// Key==GameObject的InstanceId
        /// </summary>
        private Dictionary<int, PrefabPool> m_InstanceIdPoolIdDic;

        /// <summary>
        /// Key==Prefab的InstanceId
        /// </summary>
        private Dictionary<int, AssetReferenceEntity> m_PrefabAssetDic;

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
                    m_SpawnPoolDic[entity.PoolId] = entity;
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
                    m_SpawnPoolDic[entity.PoolId] = entity;
                }
            }
        }

        public GameObjectPool()
        {
            m_SpawnPoolDic = new Dictionary<byte, GameObjectPoolEntity>();
            m_InstanceIdPoolIdDic = new Dictionary<int, PrefabPool>();
            m_PrefabAssetDic = new Dictionary<int, AssetReferenceEntity>();

            //对象池物体克隆请求
            InstanceHandler.InstantiateDelegates += InstantiateDelegate;
        }
        private GameObject InstantiateDelegate(GameObject prefab)
        {
            GameObject obj = Object.Instantiate(prefab);
            if (m_PrefabAssetDic.TryGetValue(prefab.GetInstanceID(), out AssetReferenceEntity referenceEntity))
            {
                AutoReleaseHandle.Add(referenceEntity, obj);
            }
            return obj;
        }

        /// <summary>
        /// 预加载对象池
        /// </summary>
        public void PreloadObj(PrefabName prefabName, int count)
        {
            List<GameObject> preloadList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject poolObj = Spawn(prefabName);
                preloadList.Add(poolObj);
            }
            for (int i = 0; i < count; i++)
            {
                Despawn(preloadList[i]);
            }
        }
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
        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        public async UniTask<GameObject> SpawnAsync(PrefabName prefabName)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName.ToString());
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
            int prefabId = prefab.gameObject.GetInstanceID();
            m_PrefabAssetDic[prefabId] = referenceEntity;

            return Spawn(prefab, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }

        public GameObject Spawn(PrefabName prefabName)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName.ToString());
            return Spawn(sys_Prefab);
        }
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
            int prefabId = prefab.gameObject.GetInstanceID();
            m_PrefabAssetDic[prefabId] = referenceEntity;

            return Spawn(prefab, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }

        public GameObject Spawn(GameObject prefab, byte poolId = 1, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 10, int cullMaxPerPass = 0)
        {
            if (prefab == null)
            {
                return null;
            }
            //拿到对象池
            GameObjectPoolEntity gameObjectPoolEntity = m_SpawnPoolDic[poolId];

            PrefabPool prefabPoolInner = gameObjectPoolEntity.Pool.GetPrefabPool(prefab);
            if (prefabPoolInner == null)
            {
                //对象池配置
                prefabPoolInner = new PrefabPool(prefab);
                prefabPoolInner.cullDespawned = cullDespawned;
                prefabPoolInner.cullAbove = cullAbove;
                prefabPoolInner.cullDelay = cullDelay;
                prefabPoolInner.cullMaxPerPass = cullMaxPerPass;

                gameObjectPoolEntity.Pool.CreatePrefabPool(prefabPoolInner);
            }

            //拿到一个实例
            bool isNewInstance = false;

            GameObject inst = prefabPoolInner.SpawnInstance(ref isNewInstance);
            inst.transform.SetParent(prefabPoolInner.spawnPool.transform, false);
            inst.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            //把实例映射到字典上
            int instanceID = inst.GetInstanceID();
            m_InstanceIdPoolIdDic[instanceID] = prefabPoolInner;

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
            if (m_InstanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                if (prefabPool.spawnPool == null) return;

                m_InstanceIdPoolIdDic.Remove(instanceID);

                inst.transform.SetParent(prefabPool.spawnPool.transform);
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
            if (m_InstanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                m_InstanceIdPoolIdDic.Remove(instanceID);

                prefabPool.Release(inst);
            }
            else
            {
                GameEntry.LogError(LogCategory.Pool, "该对象不在池内, inst==" + inst);
            }
        }

    }
}
//===================================================
using YouYou;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

namespace YouYou
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
        private GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Object.Instantiate(prefab, pos, rot);
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
            List<PoolObj> preloadList = new List<PoolObj>();
            for (int i = 0; i < count; i++)
            {
                PoolObj poolObj = Spawn(prefabName);
                preloadList.Add(poolObj);
            }
            for (int i = 0; i < count; i++)
            {
                Despawn(preloadList[i]);
            }
        }
        public void PreloadObj(Transform prefab, int count)
        {
            List<PoolObj> preloadList = new List<PoolObj>();
            for (int i = 0; i < count; i++)
            {
                PoolObj poolObj = Spawn(prefab);
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
        public async UniTask<PoolObj> SpawnAsync(PrefabName prefabName, Transform panent = null)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName.ToString());
            return await SpawnAsync(sys_Prefab, panent);
        }
        public async UniTask<PoolObj> SpawnAsync(Sys_PrefabEntity entity, Transform panent = null)
        {
            AssetReferenceEntity referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(entity.AssetFullPath);
            GameObject retObj = referenceEntity.Target as GameObject;
            if (retObj == null)
            {
                YouYou.GameEntry.LogError(LogCategory.Loader, "找不到Prefab, AssetFullName==" + entity.AssetFullPath);
                return null;
            }
            Transform prefab = retObj.transform;
            int prefabId = prefab.gameObject.GetInstanceID();
            m_PrefabAssetDic[prefabId] = referenceEntity;

            return Spawn(prefab, panent, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }

        public PoolObj Spawn(PrefabName prefabName, Transform panent = null)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName.ToString());
            return Spawn(sys_Prefab, panent);
        }
        public PoolObj Spawn(string prefabName, Transform panent = null)
        {
            Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName);
            return Spawn(sys_Prefab, panent);
        }
        public PoolObj Spawn(Sys_PrefabEntity entity, Transform panent = null)
        {
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(entity.AssetFullPath);
            GameObject retObj = referenceEntity.Target as GameObject;
            if (retObj == null)
            {
                YouYou.GameEntry.LogError(LogCategory.Loader, "找不到Prefab, AssetFullPath==" + entity.AssetFullPath);
                return null;
            }
            Transform prefab = retObj.transform;
            int prefabId = prefab.gameObject.GetInstanceID();
            m_PrefabAssetDic[prefabId] = referenceEntity;

            return Spawn(prefab, panent, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass);
        }

        public PoolObj Spawn(Transform prefab, Transform parent = null, byte poolId = 1, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 10, int cullMaxPerPass = 0)
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
            Transform retTrans = gameObjectPoolEntity.Pool.Spawn(prefab, ref isNewInstance);

            //初始化实例
            int instanceID = retTrans.gameObject.GetInstanceID();
            m_InstanceIdPoolIdDic[instanceID] = prefabPoolInner;
            if (parent != null)
            {
                retTrans.SetParent(parent, false);
            }
            else
            {
                retTrans.SetParent(prefabPoolInner.spawnPool.transform, false);
            }

            //初始化实例的PoolObj
            PoolObj poolObj = retTrans.gameObject.GetOrCreatComponent<PoolObj>();
            poolObj.IsNew = isNewInstance;
            if (!poolObj.IsNew) poolObj.OnOpen();
            poolObj.IsActive = true;
            poolObj.BeginTime();

            return poolObj;
        }
        #endregion

        #region Despawn 对象回池
        /// <summary>
        /// 对象回池
        /// </summary>
        public void Despawn(Transform instance)
        {
            if (instance == null) return;

            PoolObj poolObj = instance.GetComponent<PoolObj>();
            Despawn(poolObj);
        }
        public void Despawn(PoolObj poolObj)
        {
            if (poolObj == null) return;

            int instanceID = poolObj.gameObject.GetInstanceID();
            if (m_InstanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                if (prefabPool.spawnPool == null) return;

                m_InstanceIdPoolIdDic.Remove(instanceID);

                poolObj.OnClose();
                poolObj.StopTime();
                poolObj.transform.SetParent(prefabPool.spawnPool.transform);
                prefabPool.DespawnInstance(poolObj.transform);
            }
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
                            Despawn(item);
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 直接释放对象
        /// </summary>
        public void Release(Transform instance)
        {
            int instanceID = instance.gameObject.GetInstanceID();
            if (m_InstanceIdPoolIdDic.TryGetValue(instanceID, out PrefabPool prefabPool))
            {
                m_InstanceIdPoolIdDic.Remove(instanceID);

                prefabPool.Release(instance);
            }
        }

    }
}
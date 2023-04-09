//===================================================
using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 游戏物体对象池
    /// </summary>
    public class GameObjectPool : IDisposable
    {
        /// <summary>
        /// 游戏物体对象池字典
        /// </summary>
        public Dictionary<byte, GameObjectPoolEntity> m_SpawnPoolDic;

        /// <summary>
        /// 实例ID对应对象池ID
        /// </summary>
        private Dictionary<int, PrefabPool> m_InstanceIdPoolIdDic;

        /// <summary>
        /// 空闲预设池队列 相当于对这个预设池再加了一层池
        /// </summary>
        private Queue<PrefabPool> m_PrefabPoolQueue;

        /// <summary>
        /// Key==Prefab的InstanceId
        /// </summary>
        private Dictionary<int, ResourceEntity> m_PrefabResourceDic;

        private readonly Dictionary<Type, Queue<Object>> pool = new Dictionary<Type, Queue<Object>>();

        public GameObject YouYouObjPool { get; private set; }


        internal void Init()
        {
            if (YouYouObjPool == null) YouYouObjPool = new GameObject("YouYouObjPool");

            for (int i = 0; i < GameEntry.Instance.GameObjectPoolGroups.Length; i++)
            {
                GameObjectPoolEntity entity = GameEntry.Instance.GameObjectPoolGroups[i];

                //创建对象池
                SpawnPool pool = PathologicalGames.PoolManager.Pools.Create(entity.PoolName);
                pool.group.SetParent(YouYouObjPool.transform);
                pool.group.localPosition = Vector3.zero;
                pool.group.localPosition = Vector3.zero;
                entity.Pool = pool;

                m_SpawnPoolDic[entity.PoolId] = entity;
            }
        }

        public void Dispose()
        {
            m_SpawnPoolDic.Clear();
        }
        public GameObjectPool()
        {
            m_SpawnPoolDic = new Dictionary<byte, GameObjectPoolEntity>();
            m_InstanceIdPoolIdDic = new Dictionary<int, PrefabPool>();
            m_PrefabPoolQueue = new Queue<PrefabPool>();
            m_PrefabResourceDic = new Dictionary<int, ResourceEntity>();

            //对象池物体克隆请求
            InstanceHandler.InstantiateDelegates += InstantiateDelegate;
            //对象池物体销毁请求
            InstanceHandler.DestroyDelegates += DestroyDelegate;
        }
        private GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot, object userData)
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefab, pos, rot);
            if (m_PrefabResourceDic.TryGetValue(prefab.GetInstanceID(), out ResourceEntity resourceEntity))
            {
                GameEntry.Pool.RegisterInstanceResource(obj.GetInstanceID(), resourceEntity);
            }
            return obj;
        }
        private void DestroyDelegate(GameObject instance)
        {
            UnityEngine.Object.Destroy(instance);
            GameEntry.Pool.ReleaseInstanceResource(instance.GetInstanceID());
        }

        public async void PreloadObj(Transform prefab, int count)
        {
            List<PoolObj> aaa = new List<PoolObj>();
            for (int i = 0; i < count; i++)
            {
                PoolObj poolObj = await Spawn(prefab);
                aaa.Add(poolObj);
            }
            for (int i = 0; i < count; i++)
            {
                Despawn(aaa[i]);
            }
        }

        #region Spawn 从对象池中获取对象
        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        public async ETTask<PoolObj> Spawn(string prefabName, Transform panent = null)
        {
            ETTask<PoolObj> task = ETTask<PoolObj>.Create();
            SpawnAction(prefabName, panent, trans => task.SetResult(trans));
            return await task;
        }
        public async ETTask<PoolObj> Spawn(Transform prefab, Transform panent = null)
        {
            ETTask<PoolObj> task = ETTask<PoolObj>.Create();
            SpawnAction(prefab, panent, onComplete: trans => task.SetResult(trans));
            return await task;
        }
        public void SpawnAction(string prefabName, Transform panent = null, Action<PoolObj> onComplete = null)
        {
            Sys_PrefabEntity sys_PrefabEntity = GameEntry.DataTable.Sys_PrefabDBModel.GetEntityByName(prefabName);
            if (sys_PrefabEntity == null)
            {
                YouYou.GameEntry.LogError(LogCategory.Resource, "sys_PrefabEntity == null, prefabName==" + prefabName);
                return;
            }
            SpawnAction(sys_PrefabEntity, panent, onComplete);
        }
        public async void SpawnAction(Sys_PrefabEntity entity, Transform panent = null, Action<PoolObj> onComplete = null)
        {
            ResourceEntity resourceEntity = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync(entity.AssetPath);
            GameObject retObj = resourceEntity.Target as GameObject;
            if (retObj == null)
            {
                YouYou.GameEntry.LogError(LogCategory.Resource, "找不到Prefab, AssetFullName==" + entity.AssetPath);
                onComplete?.Invoke(null);
                return;
            }
            Transform prefab = retObj.transform;
            int prefabId = prefab.gameObject.GetInstanceID();
            m_PrefabResourceDic[prefabId] = resourceEntity;

            SpawnAction(prefab, panent, entity.PoolId, entity.CullDespawned == 1, entity.CullAbove, entity.CullDelay, entity.CullMaxPerPass, onComplete);
        }
        public void SpawnAction(Transform prefab, Transform panent = null, byte poolId = 1, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 10, int cullMaxPerPass = 0, Action<PoolObj> onComplete = null)
        {
            if (prefab == null)
            {
                onComplete?.Invoke(null);
                return;
            }
            //拿到对象池
            GameObjectPoolEntity gameObjectPoolEntity = m_SpawnPoolDic[poolId];

            PrefabPool prefabPoolInner = gameObjectPoolEntity.Pool.GetPrefabPool(prefab);
            if (prefabPoolInner == null)
            {
                //先去队列里找 空闲的池
                if (m_PrefabPoolQueue.Count > 0)
                {
                    prefabPoolInner = m_PrefabPoolQueue.Dequeue();

                    gameObjectPoolEntity.Pool.AddPrefabPool(prefabPoolInner);

                    prefabPoolInner.prefab = prefab;
                    prefabPoolInner.prefabGO = prefab.gameObject;
                    prefabPoolInner.AddPrefabToDic(prefab.name, prefab);
                }
                else
                {
                    prefabPoolInner = new PrefabPool(prefab);
                    gameObjectPoolEntity.Pool.CreatePrefabPool(prefabPoolInner);
                }

                prefabPoolInner.OnPrefabPoolClear = (PrefabPool pool) =>
                {
                    //预设池加入队列
                    gameObjectPoolEntity.Pool.RemovePrefabPool(pool);
                    m_PrefabPoolQueue.Enqueue(pool);
                };

                //对象池配置
                prefabPoolInner.cullDespawned = cullDespawned;
                prefabPoolInner.cullAbove = cullAbove;
                prefabPoolInner.cullDelay = cullDelay;
                prefabPoolInner.cullMaxPerPass = cullMaxPerPass;
            }

            //拿到一个实例
            bool isNewInstance = false;
            Transform retTrans = gameObjectPoolEntity.Pool.Spawn(prefab, ref isNewInstance);
            InitObj(retTrans, panent, prefab.gameObject, prefabPoolInner);

            PoolObj poolObj = retTrans.gameObject.GetOrCreatComponent<PoolObj>();
            poolObj.IsNew = isNewInstance;
            poolObj.IsActive = true;
            poolObj.BeginTime();

            onComplete?.Invoke(poolObj);
        }
        void InitObj(Transform retTrans, Transform pannt, GameObject prefab, PrefabPool prefabPool)
        {
            int instanceID = retTrans.gameObject.GetInstanceID();
            m_InstanceIdPoolIdDic[instanceID] = prefabPool;

            if (pannt != null) retTrans.SetParent(pannt);
            if (prefab != null)
            {
                retTrans.localPosition = prefab.transform.localPosition;
                retTrans.localScale = prefab.transform.localScale;
                retTrans.localEulerAngles = prefab.transform.localEulerAngles;
            }
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

        public T GetObject<T>(Object ifCreatObject, Transform ifCreatObjectParent = null) where T : Object
        {
            var type = typeof(T);
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            T poolObj;
            while (queue.Count > 0) //如果池内的物体被意外删除了, 就会被清除忽略掉
            {
                poolObj = queue.Dequeue() as T;
                if (poolObj != null)
                    return poolObj;
            }
            poolObj = Object.Instantiate(ifCreatObject as T, ifCreatObjectParent);
            return poolObj;
        }

        public void Recycling(Object obj)
        {
            if (obj == null)
                return;
            var type = obj.GetType();
            if (!pool.TryGetValue(type, out var queue))
                pool.Add(type, queue = new Queue<Object>());
            queue.Enqueue(obj);
        }
    }
}
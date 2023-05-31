using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathologicalGames
{
    public static class PoolManager
    {
        public static readonly SpawnPoolsDict Pools = new SpawnPoolsDict();
    }

    public static class InstanceHandler
    {
        //[改造] 增加resourceEntity
        public delegate GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot, object resourceEntity = null);
        public delegate void DestroyDelegate(GameObject instance);

        /// <summary>
        /// 可以用来拦截Instantiate来实现你自己的处理
        /// </summary>
        public static InstantiateDelegate InstantiateDelegates;

        /// <summary>
        /// 可以用来拦截Destroys来实现你自己的处理
        /// </summary>
        public static DestroyDelegate DestroyDelegates;

        /// <summary>
        /// 克隆对象
        /// </summary>
        internal static GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot, object resourceEntity = null)
        {
            if (InstantiateDelegates != null)
            {
                return InstantiateDelegates(prefab, pos, rot, resourceEntity);
            }
            else
            {
                return Object.Instantiate(prefab, pos, rot);
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        internal static void DestroyInstance(GameObject instance)
        {
            if (DestroyDelegates != null)
            {
                DestroyDelegates(instance);
            }
            else
            {
                Object.Destroy(instance);
            }
        }
    }


    public class SpawnPoolsDict
    {
        public delegate void OnCreatedDelegate(SpawnPool pool);

        internal Dictionary<string, OnCreatedDelegate> onCreatedDelegates = new Dictionary<string, OnCreatedDelegate>();

        public void AddOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
        {
            if (!this.onCreatedDelegates.ContainsKey(poolName))
            {
                this.onCreatedDelegates.Add(poolName, createdDelegate);

                Debug.Log(string.Format(
                    "Added onCreatedDelegates for pool '{0}': {1}", poolName, createdDelegate.Target)
                );

                return;
            }

            this.onCreatedDelegates[poolName] += createdDelegate;
        }

        public void RemoveOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
        {
            if (!this.onCreatedDelegates.ContainsKey(poolName))
                throw new KeyNotFoundException
                (
                    "No OnCreatedDelegates found for pool name '" + poolName + "'."
                );

            this.onCreatedDelegates[poolName] -= createdDelegate;

            Debug.Log(string.Format(
                "Removed onCreatedDelegates for pool '{0}': {1}", poolName, createdDelegate.Target)
            );
        }


        public SpawnPool Create(string poolName)
        {
            var owner = new GameObject(poolName + "Pool");
            return owner.AddComponent<SpawnPool>();
        }



        private Dictionary<string, SpawnPool> _pools = new Dictionary<string, SpawnPool>();
        internal void Add(SpawnPool spawnPool)
        {
            if (this.ContainsKey(spawnPool.poolName))
            {
                Debug.LogError(string.Format("A pool with the name '{0}' already exists. " +
                                                "This should only happen if a SpawnPool with " +
                                                "this name is added to a scene twice.",
                                             spawnPool.poolName));
                return;
            }

            this._pools.Add(spawnPool.poolName, spawnPool);

            if (this.onCreatedDelegates.ContainsKey(spawnPool.poolName))
                this.onCreatedDelegates[spawnPool.poolName](spawnPool);
        }
        internal bool Remove(SpawnPool spawnPool)
        {
            if (!this.ContainsValue(spawnPool) & Application.isPlaying)
            {
                Debug.LogError(string.Format(
                    "PoolManager: Unable to remove '{0}'. Pool not in PoolManager",
                     spawnPool.poolName
                ));
                return false;
            }

            this._pools.Remove(spawnPool.poolName);
            return true;
        }
        public bool ContainsKey(string poolName)
        {
            return this._pools.ContainsKey(poolName);
        }
        public bool ContainsValue(SpawnPool pool)
        {
            return this._pools.ContainsValue(pool);
        }

    }

}

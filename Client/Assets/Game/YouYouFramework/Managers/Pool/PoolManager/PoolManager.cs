using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PathologicalGames
{
    public static class PoolManager
    {
        public static readonly SpawnPoolsDict Pools = new SpawnPoolsDict();
    }


    /// <summary>
    /// This can be used to intercept Instantiate and Destroy to implement your own handling. See 
    /// PoolManagerExampleFiles/Scripts/InstanceHandlerDelegateExample.cs.
    /// 
    /// Simply add your own delegate and it will be run. 
    /// 
    /// If a SpawnPool.InstantiateDelegate is used it will override the one set here.
    /// </summary>
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
        #region Event Handling
        public delegate void OnCreatedDelegate(SpawnPool pool);

        internal Dictionary<string, OnCreatedDelegate> onCreatedDelegates =
             new Dictionary<string, OnCreatedDelegate>();

        public void AddOnCreatedDelegate(string poolName, OnCreatedDelegate createdDelegate)
        {
            // Assign first delegate "just in time"
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

        #endregion Event Handling

        #region Public Custom Memebers
        /// <summary>
        /// Creates a new GameObject with a SpawnPool Component which registers itself
        /// with the PoolManager.Pools dictionary. The SpawnPool can then be accessed 
        /// directly via the return value of this function or by via the PoolManager.Pools 
        /// dictionary using a 'key' (string : the name of the pool, SpawnPool.poolName).
        /// </summary>
        /// <param name="poolName">
        /// The name for the new SpawnPool. The GameObject will have the word "Pool"
        /// Added at the end.
        /// </param>
        /// <returns>A reference to the new SpawnPool component</returns>
        public SpawnPool Create(string poolName)
        {
            // Add "Pool" to the end of the poolName to make a more user-friendly
            //   GameObject name. This gets stripped back out in SpawnPool Awake()
            var owner = new GameObject(poolName + "Pool");
            return owner.AddComponent<SpawnPool>();
        }
        #endregion Public Custom Memebers



        #region Dict Functionality
        // Internal (wrapped) dictionary
        private Dictionary<string, SpawnPool> _pools = new Dictionary<string, SpawnPool>();

        /// <summary>
        /// Used internally by SpawnPools to add themseleves on Awake().
        /// Use PoolManager.CreatePool() to create an entirely new SpawnPool GameObject
        /// </summary>
        /// <param name="spawnPool"></param>
        internal void Add(SpawnPool spawnPool)
        {
            // Don't let two pools with the same name be added. See error below for details
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


        /// <summary>
        /// Used internally by SpawnPools to remove themseleves on Destroy().
        /// Use PoolManager.Destroy() to destroy an entire SpawnPool GameObject.
        /// </summary>
        /// <param name="spawnPool"></param>
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

        /// <summary>
        /// Returns true if a pool exists with the passed pool name.
        /// </summary>
        /// <param name="poolName">The name to look for</param>
        /// <returns>True if the pool exists, otherwise, false.</returns>
        public bool ContainsKey(string poolName)
        {
            return this._pools.ContainsKey(poolName);
        }

        /// <summary>
        /// Returns true if a SpawnPool instance exists in this Pools dict.
        /// </summary>
        /// <param name="poolName">The name to look for</param>
        /// <returns>True if the pool exists, otherwise, false.</returns>
        public bool ContainsValue(SpawnPool pool)
        {
            return this._pools.ContainsValue(pool);
        }

        #endregion Dict Functionality

    }

}

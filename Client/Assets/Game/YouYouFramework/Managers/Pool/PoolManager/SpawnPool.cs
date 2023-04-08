using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathologicalGames
{
    /// <summary>
    /// 总池, 包含多个对象池
    /// </summary>
    [AddComponentMenu("Path-o-logical/PoolManager/SpawnPool")]
    public sealed class SpawnPool : MonoBehaviour
    {
        #region Inspector Parameters
        /// <summary>
        /// Returns the name of this pool used by PoolManager. This will always be the
        /// same as the name in Unity, unless the name contains the work "Pool", which
        /// PoolManager will strip out. This is done so you can name a prefab or
        /// GameObject in a way that is development friendly. For example, "EnemiesPool" 
        /// is easier to understand than just "Enemies" when looking through a project.
        /// </summary>
        public string poolName = "";

        /// <summary>
        /// If True, do not reparent instances under the SpawnPool's Transform.
        /// </summary>
        public bool dontReparent = false;

        /// <summary>
        /// Print information to the Unity Console
        /// </summary>
        public bool logMessages = false;

        #endregion Inspector Parameters



        #region Public Code-only Parameters
        /// <summary>
        /// The group is an empty game object which will be the parent of all
        /// instances in the pool. This helps keep the scene easy to work with.
        /// </summary>
        public Transform group { get; private set; }

        // Returns the prefab of the given name (dictionary key)
        public Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();

        // Keeps the state of each individual foldout item during the editor session
        public Dictionary<object, bool> _editorListItemStates = new Dictionary<object, bool>();

        /// <summary>
        /// Readonly access to prefab pools via a dictionary<string, PrefabPool>.
        /// </summary>
        public Dictionary<string, PrefabPool> prefabPools
        {
            get
            {
                var dict = new Dictionary<string, PrefabPool>();

                for (int i = 0; i < this._prefabPools.Count; i++)
                    dict[this._prefabPools[i].prefabGO.name] = this._prefabPools[i];

                return dict;
            }
        }
        #endregion Public Code-only Parameters


        private List<PrefabPool> _prefabPools = new List<PrefabPool>();


        #region 构造函数和初始化
        private void OnDestroy()
        {
            //进行清理
            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0}: Destroying...", this.poolName));

            if (PoolManager.Pools.ContainsValue(this))
                PoolManager.Pools.Remove(this);

            this.StopAllCoroutines();

            // Clean-up
            foreach (PrefabPool pool in this._prefabPools)
            {
                pool.SelfDestruct();
            }

            // Probably overkill, and may not do anything at all, but...
            this._prefabPools.Clear();
            this.prefabs.Clear();
        }
        private void Awake()
        {
            this.group = this.transform;

            // Default name behavior will use the GameObject's name without "Pool" (if found)
            if (this.poolName == "")
            {
                // Automatically Remove "Pool" from names to allow users to name prefabs in a 
                //   more development-friendly way. E.g. "EnemiesPool" becomes just "Enemies".
                //   Notes: This will return the original string if "Pool" isn't found.
                //          Do this once here, rather than a getter, to avoide string work
                this.poolName = this.group.name.Replace("Pool", "");
                this.poolName = this.poolName.Replace("(Clone)", "");
            }


            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0}: Initializing..", this.poolName));

            // Add this SpawnPool to PoolManager for use. This is done last to lower the 
            //   possibility of adding a badly init pool.
            PoolManager.Pools.Add(this);
        }



        public delegate GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot);
        public delegate void DestroyDelegate(GameObject instance);

        /// <summary>
        /// 可以用来拦截Instantiate来实现你自己的处理
        /// </summary>
        public InstantiateDelegate instantiateDelegates;

        /// <summary>
        /// 可以用来拦截Destroy来实现你自己的处理
        /// </summary>
        public DestroyDelegate destroyDelegates;

        /// <summary>
        /// 克隆对象
        /// </summary>
        internal GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (this.instantiateDelegates != null)
            {
                return this.instantiateDelegates(prefab, pos, rot);
            }
            else
            {
                return InstanceHandler.InstantiatePrefab(prefab, pos, rot);
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        internal void DestroyInstance(GameObject instance)
        {
            if (this.destroyDelegates != null)
            {
                this.destroyDelegates(instance);
            }
            else
            {
                InstanceHandler.DestroyInstance(instance);
            }
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        public void CreatePrefabPool(PrefabPool prefabPool)
        {
            bool isAlreadyPool = this.GetPrefabPool(prefabPool.prefab) == null ? false : true;
            if (isAlreadyPool)
                throw new System.Exception(string.Format
                (
                    "Prefab '{0}' is already in  SpawnPool '{1}'. Prefabs can be in more than 1 SpawnPool but " +
                    "cannot be in the same SpawnPool twice.",
                    prefabPool.prefab,
                    this.poolName
                ));

            // Used internally to reference back to this spawnPool for things 
            //   like anchoring co-routines.
            prefabPool.spawnPool = this;

            this._prefabPools.Add(prefabPool);

            // Add to the prefabs dict for convenience
            this.prefabs.Add(prefabPool.prefab.name, prefabPool.prefab);
        }
        #endregion 构造函数和初始化


        #region 池功能, 取对象, 回对象
        public Transform Spawn(Transform prefab, Vector3 pos, Quaternion rot, Transform parent, ref bool isNewInstance)
        {
            Transform inst;
            bool worldPositionStays;

            #region Use from Pool
            for (int i = 0; i < this._prefabPools.Count; i++)
            {
                // Determine if the prefab was ever used as explained in the docs
                //   I believe a comparison of two references is processor-cheap.
                if (this._prefabPools[i].prefab == prefab)
                {
                    // Now we know the prefabPool for this prefab exists. 
                    // Ask the prefab pool to setup and activate an instance.
                    // If there is no instance to spawn, a new one is instanced
                    inst = this._prefabPools[i].SpawnInstance(pos, rot, ref isNewInstance);

                    // This only happens if the limit option was used for this
                    //   Prefab Pool.
                    if (inst == null) return null;

                    // This will handle RectTransforms as well
                    worldPositionStays = !(inst is RectTransform);

                    if (parent != null)  // User explicitly provided a parent
                    {
                        inst.SetParent(parent, worldPositionStays);
                    }
                    else if (!this.dontReparent && inst.parent != this.group)  // Auto organize?
                    {
                        // If a new instance was created, it won't be grouped
                        inst.SetParent(this.group, worldPositionStays);
                    }

                    // Done!
                    return inst;
                }
            }
            #endregion Use from Pool


            #region New PrefabPool
            // The prefab wasn't found in any PrefabPools above. Make a new one
            PrefabPool newPrefabPool = new PrefabPool(prefab);
            this.CreatePrefabPool(newPrefabPool);

            // Spawn the new instance (Note: prefab already set in PrefabPool)
            inst = newPrefabPool.SpawnInstance(pos, rot, ref isNewInstance);
            worldPositionStays = !(inst is RectTransform);
            if (parent != null)  // User explicitly provided a parent
            {
                inst.SetParent(parent, worldPositionStays);
            }
            else if (!this.dontReparent && inst.parent != this.group)  // Auto organize?
            {
                // If a new instance was created, it won't be grouped
                inst.SetParent(this.group, worldPositionStays);
            }

            #endregion New PrefabPool

            // Done!
            return inst;
        }
        public Transform Spawn(Transform prefab, Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            Transform inst = this.Spawn(prefab, pos, rot, null, ref isNewInstance);

            // Can happen if limit was used
            if (inst == null) return null;

            return inst;
        }
        public Transform Spawn(Transform prefab, ref bool isNewInstance)
        {
            return this.Spawn(prefab, Vector3.zero, Quaternion.identity, ref isNewInstance);
        }
        public Transform Spawn(Transform prefab, Transform parent, ref bool isNewInstance)
        {
            return this.Spawn(prefab, Vector3.zero, Quaternion.identity, parent, ref isNewInstance);
        }


        public Transform Spawn(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, ref bool isNewInstance)
        {
            return Spawn(prefab.transform, pos, rot, parent, ref isNewInstance);
        }
        public Transform Spawn(GameObject prefab, Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            return Spawn(prefab.transform, pos, rot, ref isNewInstance);
        }
        public Transform Spawn(GameObject prefab, ref bool isNewInstance)
        {
            return Spawn(prefab.transform, ref isNewInstance);
        }
        public Transform Spawn(GameObject prefab, Transform parent, ref bool isNewInstance)
        {
            return Spawn(prefab.transform, parent, ref isNewInstance);
        }


        public Transform Spawn(string prefabName, ref bool isNewInstance)
        {
            Transform prefab = this.prefabs[prefabName];
            return this.Spawn(prefab, ref isNewInstance);
        }
        public Transform Spawn(string prefabName, Transform parent, ref bool isNewInstance)
        {
            Transform prefab = this.prefabs[prefabName];
            return this.Spawn(prefab, parent, ref isNewInstance);
        }
        public Transform Spawn(string prefabName, Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            Transform prefab = this.prefabs[prefabName];
            return this.Spawn(prefab, pos, rot, ref isNewInstance);
        }
        public Transform Spawn(string prefabName, Vector3 pos, Quaternion rot,
                               Transform parent, ref bool isNewInstance)
        {
            Transform prefab = this.prefabs[prefabName];
            return this.Spawn(prefab, pos, rot, parent, ref isNewInstance);
        }

        #endregion Pool Functionality


        #region Utility Functions
        /// <summary>
        /// Returns the prefab pool for a given prefab.
        /// </summary>
        /// <param name="prefab">The Transform of an instance</param>
        /// <returns>PrefabPool</returns>
        public PrefabPool GetPrefabPool(Transform prefab)
        {
            for (int i = 0; i < this._prefabPools.Count; i++)
            {
                if (this._prefabPools[i].prefab == null)
                    Debug.LogError(string.Format("SpawnPool {0}: PrefabPool.prefabGO is null",
                                                 this.poolName));

                if (this._prefabPools[i].prefab == prefab)
                    return this._prefabPools[i];
            }

            // Nothing found
            return null;
        }

        /// <summary>
        /// 从列表里移除预设池, 改造
        /// </summary>
        public void RemovePrefabPool(PrefabPool prefabPool)
        {
            this._prefabPools.Remove(prefabPool);
        }

        /// <summary>
        /// 把预设池加入列表, 改造
        /// </summary>
        /// <param name="prefabPool"></param>
        public void AddPrefabPool(PrefabPool prefabPool)
        {
            this._prefabPools.Add(prefabPool);
        }

        #endregion Utility Functions

    }

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PathologicalGames
{
    /// <summary>
    /// 总池, 包含多个对象池
    /// </summary>
    public sealed class SpawnPool : MonoBehaviour
    {
        public string poolName = "";

        private List<PrefabPool> _prefabPools = new List<PrefabPool>();


        private void OnDestroy()
        {
            //进行清理
            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0}: Destroying...", this.poolName));

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
        }
        private void Awake()
        {
            if (this.poolName == "")
            {
                this.poolName = this.transform.name.Replace("Pool", "");
                this.poolName = this.poolName.Replace("(Clone)", "");
            }


            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0}: Initializing..", this.poolName));

            // Add this SpawnPool to PoolManager for use. This is done last to lower the 
            //   possibility of adding a badly init pool.
            PoolManager.Pools.Add(this);
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
        }
        /// <summary>
        /// 取对象池
        /// </summary>
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
            return null;
        }

        /// <summary>
        /// 取对象
        /// </summary>
        public Transform Spawn(Transform prefab, Vector3 pos, Quaternion rot, Transform parent, ref bool isNewInstance)
        {
            Transform inst;
            bool worldPositionStays;

            //从已有对象池拿对象 
            PrefabPool prefabPool = null;
            for (int i = 0; i < this._prefabPools.Count; i++)
            {
                if (this._prefabPools[i].prefab == prefab)
                {
                    prefabPool = _prefabPools[i];
                    break;
                }
            }

            //如果对象池不存在， 创建新的对象池
            if (prefabPool == null)
            {
                prefabPool = new PrefabPool(prefab);
                this.CreatePrefabPool(prefabPool);
            }

            //生成新实例
            inst = prefabPool.SpawnInstance(pos, rot, ref isNewInstance);
            worldPositionStays = !(inst is RectTransform);// 如果不是UI， 保持世界坐标
            if (parent != null)//用户显式地提供了父节点
            {
                inst.SetParent(parent, worldPositionStays);
            }

            return inst;
        }
        public Transform Spawn(Transform prefab, ref bool isNewInstance)
        {
            return this.Spawn(prefab, Vector3.zero, Quaternion.identity, null, ref isNewInstance);
        }

    }

}
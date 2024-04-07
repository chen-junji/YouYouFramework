using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace YouYouFramework
{
    /// <summary>
    /// 总池, 包含多个对象池
    /// </summary>
    public sealed class SpawnPool : MonoBehaviour
    {
        private List<PrefabPool> prefabPools = new List<PrefabPool>();


        private void OnDestroy()
        {
            StopAllCoroutines();

            foreach (PrefabPool pool in prefabPools)
            {
                pool.SelfDestruct();
            }
            prefabPools.Clear();
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        public void CreatePrefabPool(PrefabPool prefabPool)
        {
            if (GetPrefabPool(prefabPool.prefab) != null)
            {
                GameEntry.Log(LogCategory.Pool, "该Prefab对应的对象池已经存在， 不要重复创建!  prefab==" + prefabPool.prefab);
                return;
            }

            prefabPool.spawnPool = this;
            prefabPools.Add(prefabPool);
        }
        /// <summary>
        /// 取对象池
        /// </summary>
        public PrefabPool GetPrefabPool(GameObject prefab)
        {
            for (int i = 0; i < prefabPools.Count; i++)
            {
                if (prefabPools[i].prefab == prefab)
                {
                    return prefabPools[i];
                }
            }
            return null;
        }
    }
}
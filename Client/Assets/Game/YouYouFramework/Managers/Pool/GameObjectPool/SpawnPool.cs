using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace YouYouFramework
{
    /// <summary>
    /// 不同的分类一个池, 包含多个对象池
    /// </summary>
    public sealed class SpawnPool : MonoBehaviour
    {
        private Dictionary<int, PrefabPool> prefabPoolDic = new Dictionary<int, PrefabPool>();


        private void OnDestroy()
        {
            StopAllCoroutines();

            foreach (var pool in prefabPoolDic)
            {
                pool.Value.SelfDestruct();
            }
            prefabPoolDic.Clear();
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        public void AddPrefabPool(PrefabPool prefabPool)
        {
            int instanceID = prefabPool.prefab.GetInstanceID();
            if (prefabPoolDic.ContainsKey(instanceID))
            {
                GameEntry.Log(LogCategory.Pool, "该Prefab对应的对象池已经存在， 不要重复创建!  prefab==" + prefabPool.prefab);
                return;
            }

            prefabPool.spawnPool = this;
            prefabPoolDic.Add(instanceID, prefabPool);
        }
        /// <summary>
        /// 取对象池
        /// </summary>
        public PrefabPool GetPrefabPool(GameObject prefab)
        {
            prefabPoolDic.TryGetValue(prefab.GetInstanceID(), out PrefabPool prefabPool);
            return prefabPool;
        }
    }
}
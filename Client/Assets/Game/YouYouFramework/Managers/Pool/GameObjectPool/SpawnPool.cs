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
        public string poolName = "";

        private List<PrefabPool> _prefabPools = new List<PrefabPool>();


        private void OnDestroy()
        {
            StopAllCoroutines();

            foreach (PrefabPool pool in _prefabPools)
            {
                pool.SelfDestruct();
            }
            _prefabPools.Clear();
        }
        private void Awake()
        {
            if (poolName == "")
            {
                poolName = transform.name.Replace("Pool", "");
                poolName = poolName.Replace("(Clone)", "");
            }
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        public void CreatePrefabPool(PrefabPool prefabPool)
        {
            if (GetPrefabPool(prefabPool.prefab) != null)
            {
                YouYouFramework.GameEntry.Log(YouYouFramework.LogCategory.Pool, "该Prefab对应的对象池已经存在， 不要重复创建!  prefab==" + prefabPool.prefab);
                return;
            }

            prefabPool.spawnPool = this;
            _prefabPools.Add(prefabPool);
        }
        /// <summary>
        /// 取对象池
        /// </summary>
        public PrefabPool GetPrefabPool(Transform prefab)
        {
            for (int i = 0; i < _prefabPools.Count; i++)
            {
                if (_prefabPools[i].prefab == prefab) return _prefabPools[i];
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
            for (int i = 0; i < _prefabPools.Count; i++)
            {
                if (_prefabPools[i].prefab == prefab)
                {
                    prefabPool = _prefabPools[i];
                    break;
                }
            }

            //如果对象池不存在， 创建新的对象池
            if (prefabPool == null)
            {
                prefabPool = new PrefabPool(prefab);
                CreatePrefabPool(prefabPool);
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
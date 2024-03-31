using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 对象池
    /// </summary>
    [System.Serializable]
    public class PrefabPool
    {
        /// <summary>
        /// 预制件
        /// </summary>
        public Transform prefab;

        /// <summary>
        /// 基于性能原因而存储的预制件GameObject的引用
        /// </summary>
        public GameObject prefabGO;

        /// <summary>
        /// 是否开启缓存池自动清理模式
        /// </summary>
        public bool cullDespawned = false;

        /// <summary>
        /// 缓存池自动清理但是始终保留几个对象不清理
        /// </summary>
        public int cullAbove = 50;

        /// <summary>
        /// 多长时间清理一次单位是秒
        /// </summary>
        public int cullDelay = 60;

        /// <summary>
        /// 每次清理几个
        /// </summary>
        public int cullMaxPerPass = 5;

        /// <summary>
        /// 总池的引用
        /// </summary>
        public SpawnPool spawnPool;


        public PrefabPool(Transform prefab)
        {
            this.prefab = prefab;
            prefabGO = prefab.gameObject;
        }
        /// <summary>
        /// 销毁自身对象池
        /// </summary>
        internal void SelfDestruct()
        {
            foreach (Transform inst in _despawned)
                if (inst != null && spawnPool != null)
                    InstanceHandler.DestroyInstance(inst.gameObject);

            foreach (Transform inst in _spawned)
                if (inst != null && spawnPool != null)
                    InstanceHandler.DestroyInstance(inst.gameObject);

            _spawned.Clear();
            _despawned.Clear();

            prefab = null;
            prefabGO = null;
            spawnPool = null;

        }


        /// <summary>
        /// 定时清理协程是否正在运行中？
        /// </summary>
        private bool cullingActive = false;

        /// <summary>
        /// 已被取池的对象
        /// </summary>
        internal LinkedList<Transform> _spawned = new LinkedList<Transform>();
        /// <summary>
        /// 在池内的对象
        /// </summary>
        internal LinkedList<Transform> _despawned = new LinkedList<Transform>();

        /// <summary>
        /// 当前对象的最大数量（包括在池内的, 已被取池的）
        /// </summary>
        public int TotalCount
        {
            get
            {
                int count = 0;
                count += _spawned.Count;
                count += _despawned.Count;
                return count;
            }
        }

        /// <summary>
        /// 从池内取对象，如果没有则克隆新的
        /// </summary>
        internal Transform SpawnInstance(Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            isNewInstance = false;

            Transform inst;

            if (_despawned.Count == 0)
            {
                //池内没对象了，克隆新对象
                isNewInstance = true;
                inst = SpawnNew(pos, rot);
            }
            else
            {
                //从池里拿对象
                inst = _despawned.First.Value;
                _despawned.RemoveFirst();

                if (inst == null)
                {
                    YouYouFramework.GameEntry.Log(YouYouFramework.LogCategory.Pool, "池内拿出来的对象是null， 被私自Destroy了, Prefab==" + prefab);
                    return null;
                }

                _spawned.AddLast(inst);

                inst.position = pos;
                inst.rotation = rot;
                inst.gameObject.SetActive(true);

            }
            return inst;
        }
        /// <summary>
        /// 克隆新的对象
        /// </summary>
        private Transform SpawnNew(Vector3 pos, Quaternion rot)
        {
            if (pos == Vector3.zero) pos = spawnPool.transform.position;
            if (rot == Quaternion.identity) rot = spawnPool.transform.rotation;

            //使用InstanceHandler，克隆对象
            GameObject instGO = InstanceHandler.InstantiatePrefab(prefabGO, pos, rot);
            Transform inst = instGO.transform;
            _spawned.AddLast(inst);

#if UNITY_EDITOR
            //对象名字后缀
            inst.name += (TotalCount + 1).ToString("#000");
#endif
            return inst;
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public bool DespawnInstance(Transform xform)
        {
            _spawned.Remove(xform);
            _despawned.AddLast(xform);

            xform.gameObject.SetActive(false);

            if (!cullingActive && cullDespawned && TotalCount > cullAbove)
            {
                cullingActive = true;
                spawnPool.StartCoroutine(CullDespawned());
            }
            return true;
        }

        /// <summary>
        /// 定时清理对象池的协程
        /// </summary>
        internal IEnumerator CullDespawned()
        {
            while (TotalCount > cullAbove)
            {
                yield return new WaitForSeconds(cullDelay);

                //每次清理几个
                for (int i = 0; i < cullMaxPerPass; i++)
                {
                    //保留几个对象
                    if (TotalCount <= cullAbove) break;
                    if (_despawned.Count == 0) break;

                    Transform inst = _despawned.Last.Value;
                    _despawned.RemoveLast();
                    InstanceHandler.DestroyInstance(inst.gameObject);
                }
            }
            cullingActive = false;
            yield return null;
        }

        /// <summary>
        /// 直接完全释放
        /// </summary>
        public void Release(Transform xform)
        {
            _spawned.Remove(xform);
            InstanceHandler.DestroyInstance(xform.gameObject);
        }

    }
}
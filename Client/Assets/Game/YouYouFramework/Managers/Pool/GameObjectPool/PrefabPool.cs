using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 回池方式
    /// </summary>
    public enum SpawnType
    {
        Active,
        Position
    }

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

        public SpawnType SpawnType;

        /// <summary>
        /// 是否限制对象的最大数量
        /// </summary>
        public bool limitInstances = false;

        /// <summary>
        /// 限制对象的最大数量
        /// </summary>
        public int limitAmount = 100;

        /// <summary>
        /// 如果为True， 当对象超过最大数量， 销毁第一个对象
        /// </summary>
        public bool limitFIFO = false;

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
            if (limitInstances && limitFIFO && _spawned.Count >= limitAmount)
            {
                //当对象超过最大数量， 销毁第一个对象
                Transform firstIn = _spawned.First.Value;

                DespawnInstance(firstIn);
            }

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
                    YouYou.GameEntry.Log(YouYou.LogCategory.Pool, "池内拿出来的对象是null， 被私自Destroy了, Prefab==" + prefab);
                    return null;
                }

                _spawned.AddLast(inst);

                inst.position = pos;
                inst.rotation = rot;
                if (SpawnType == SpawnType.Active) inst.gameObject.SetActive(true);

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

            nameInstance(inst);
            return inst;
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public bool DespawnInstance(Transform xform)
        {
            _spawned.Remove(xform);
            _despawned.AddLast(xform);

            //对象回池类型
            switch (SpawnType)
            {
                case SpawnType.Active:
                    xform.gameObject.SetActive(false);
                    break;
                case SpawnType.Position:
                    xform.position = Vector3.down * 100;
                    break;
            }

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


        /// <summary>
        /// 对象名字后缀
        /// </summary>
        private void nameInstance(Transform instance)
        {
#if UNITY_EDITOR
            instance.name += (TotalCount + 1).ToString("#000");
#endif
        }
    }
}
using System;
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
        public GameObject prefab;

        /// <summary>
        /// 是否开启缓存池自动清理模式
        /// </summary>
        public bool cullDespawned;

        /// <summary>
        /// 缓存池自动清理但是始终保留几个对象不清理
        /// </summary>
        public int cullAbove;

        /// <summary>
        /// 多长时间清理一次单位是秒
        /// </summary>
        public int cullDelay;

        /// <summary>
        /// 每次清理几个
        /// </summary>
        public int cullMaxPerPass;

        /// <summary>
        /// 主要用于SetParent
        /// </summary>
        public MonoBehaviour Root;

        /// <summary>
        /// 定时清理协程是否正在运行中？
        /// </summary>
        private bool cullingActive = false;

        /// <summary>
        /// 已被取池的对象
        /// </summary>
        internal LinkedList<GameObject> spawnedList = new LinkedList<GameObject>();
        /// <summary>
        /// 在池内的对象
        /// </summary>
        internal LinkedList<GameObject> despawnedList = new LinkedList<GameObject>();

        /// <summary>
        /// 当前对象的最大数量（包括在池内的, 已被取池的）
        /// </summary>
        public int TotalCount
        {
            get
            {
                int count = 0;
                count += spawnedList.Count;
                count += despawnedList.Count;
                return count;
            }
        }

        public Func<GameObject> CreateFunc;
        public Action<GameObject> ActionOnDestroy;
        public Action ActionOnDestruct;

        public PrefabPool(GameObject prefab, bool cullDespawned = true, int cullAbove = 0, int cullDelay = 60, int cullMaxPerPass = 30)
        {
            this.prefab = prefab;
            this.cullDespawned = cullDespawned;
            this.cullAbove = cullAbove;
            this.cullDelay = cullDelay;
            this.cullMaxPerPass = cullMaxPerPass;
        }

        public void PreloadPool()
        {
            if (cullAbove > 0)
            {
                for (int i = 0; i < cullAbove; i++)
                {
                    //预加载克隆对象
                    GameObject inst = CreateFunc?.Invoke();
                    inst.SetActive(false);
                    despawnedList.AddLast(inst);

#if UNITY_EDITOR
                    //对象名字后缀
                    inst.name += (TotalCount + 1).ToString("#000");
#endif
                }
            }
        }

        /// <summary>
        /// 销毁自身对象池
        /// </summary>
        internal void SelfDestruct()
        {
            while (despawnedList.First != null)
            {
                despawnedList.RemoveFirst();
                if (despawnedList.Count > 0 && despawnedList.First.List != null && despawnedList.First.Value != null)
                {
                    ActionOnDestroy?.Invoke(despawnedList.First.Value);
                }
            }
            while (spawnedList.First != null)
            {
                spawnedList.RemoveFirst();
                if (spawnedList.Count > 0 && spawnedList.First.List != null && spawnedList.First.Value != null)
                {
                    ActionOnDestroy?.Invoke(spawnedList.First.Value);
                }
            }
            
            ActionOnDestruct?.Invoke();
            prefab = null;
            Root = null;
        }

        /// <summary>
        /// 从池内取对象，如果没有则克隆新的
        /// </summary>
        internal GameObject SpawnInstance()
        {
            GameObject inst;

            if (despawnedList.Count == 0)
            {
                //池内没对象了，克隆新对象
                inst = SpawnNew();
            }
            else
            {
                //从池里拿对象
                inst = despawnedList.First.Value;
                despawnedList.RemoveFirst();

                if (inst == null)
                {
                    GameEntry.Log(LogCategory.Pool, "池内拿出来的对象是null， 被私自Destroy了, Prefab==" + prefab);
                    return null;
                }

                spawnedList.AddLast(inst);
                inst.SetActive(true);
            }
            return inst;
        }
        /// <summary>
        /// 克隆新的对象
        /// </summary>
        private GameObject SpawnNew()
        {
            //克隆对象
            GameObject inst = CreateFunc?.Invoke();
            spawnedList.AddLast(inst);

#if UNITY_EDITOR
            //对象名字后缀
            inst.name += (TotalCount + 1).ToString("#000");
#endif
            return inst;
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public bool DespawnInstance(GameObject inst)
        {
            spawnedList.Remove(inst);
            despawnedList.AddLast(inst);

            inst.SetActive(false);

            if (!cullingActive && cullDespawned && despawnedList.Count > cullAbove)
            {
                cullingActive = true;
                Root.StartCoroutine(CullDespawned());
            }
            return true;
        }

        /// <summary>
        /// 定时清理对象池的协程
        /// </summary>
        internal IEnumerator CullDespawned()
        {
            while (despawnedList.Count > cullAbove)
            {
                yield return new WaitForSeconds(cullDelay);

                //每次清理几个
                for (int i = 0; i < cullMaxPerPass; i++)
                {
                    //保留几个对象
                    if (despawnedList.Count <= cullAbove) break;
                    if (despawnedList.Count == 0) break;

                    GameObject inst = despawnedList.Last.Value;
                    despawnedList.RemoveLast();
                    ActionOnDestroy?.Invoke(inst);
                }
            }
            cullingActive = false;
            yield return null;
        }

        /// <summary>
        /// 直接销毁
        /// </summary>
        public void Destroy(GameObject inst)
        {
            bool isRemove = spawnedList.Remove(inst);
            if (isRemove)
            {
                GameEntry.LogError(LogCategory.Pool, "该对象不在池内, inst==" + inst);
            }
            ActionOnDestroy?.Invoke(inst);
        }

    }
}
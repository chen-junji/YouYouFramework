using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PathologicalGames
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
            this.prefabGO = prefab.gameObject;
        }
        /// <summary>
        /// 销毁自身
        /// </summary>
        internal void SelfDestruct()
        {
            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0}: Cleaning up PrefabPool for {1}...", this.spawnPool.poolName, this.prefabGO.name));

            // Go through both lists and destroy everything
            foreach (Transform inst in this._despawned)
                if (inst != null && this.spawnPool != null)  // Tear-down-time protection
                    InstanceHandler.DestroyInstance(inst.gameObject);

            foreach (Transform inst in this._spawned)
                if (inst != null && this.spawnPool != null)  // Tear-down-time protection
                    InstanceHandler.DestroyInstance(inst.gameObject);

            this._spawned.Clear();
            this._despawned.Clear();

            this.prefab = null;
            this.prefabGO = null;
            this.spawnPool = null;

        }


        #region Pool Functionality
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
        public int totalCount
        {
            get
            {
                int count = 0;
                count += this._spawned.Count;
                count += this._despawned.Count;
                return count;
            }
        }


        /// <summary>
        /// 直接完全释放
        /// </summary>
        public void Release(Transform xform)
        {
            this._spawned.Remove(xform);
            InstanceHandler.DestroyInstance(xform.gameObject);
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public bool DespawnInstance(Transform xform)
        {
            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): Despawning '{2}'",
                                       this.spawnPool.poolName,
                                       this.prefab.name,
                                       xform.name));

            // Switch to the despawned list
            this._spawned.Remove(xform);
            this._despawned.AddLast(xform);

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

            // Trigger culling if the feature is ON and the size  of the 
            //   overall pool is over the Cull Above threashold.
            //   This is triggered here because Despawn has to occur before
            //   it is worth culling anyway, and it is run fairly often.
            if (!this.cullingActive &&   // Cheap & Singleton. Only trigger once!
                this.cullDespawned &&    // Is the feature even on? Cheap too.
                this.totalCount > this.cullAbove)   // Criteria met?
            {
                this.cullingActive = true;
                this.spawnPool.StartCoroutine(CullDespawned());
            }
            return true;
        }



        /// <summary>
        /// 定时清理对象池的协程
        /// </summary>
        internal IEnumerator CullDespawned()
        {
            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): CULLING TRIGGERED! " +
                                          "Waiting {2}sec to begin checking for despawns...",
                                        this.spawnPool.poolName,
                                        this.prefab.name,
                                        this.cullDelay));

            // First time always pause, then check to see if the condition is
            //   still true before attempting to cull.
            //yield return new WaitForSeconds(this.cullDelay);

            while (this.totalCount > this.cullAbove)
            {

                // Attempt to delete an amount == this.cullMaxPerPass
                for (int i = 0; i < this.cullMaxPerPass; i++)
                {
                    // Break if this.cullMaxPerPass would go past this.cullAbove
                    if (this.totalCount <= this.cullAbove)
                        break;  // The while loop will stop as well independently

                    // Destroy the last item in the list
                    if (this._despawned.Count > 0)
                    {
                        Transform inst = this._despawned.First.Value;
                        this._despawned.RemoveFirst();
                        InstanceHandler.DestroyInstance(inst.gameObject);

                        YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): " +
                                                    "CULLING to {2} instances. Now at {3}.",
                                                this.spawnPool.poolName,
                                                this.prefab.name,
                                                this.cullAbove,
                                                this.totalCount));

                    }
                    else
                    {
                        YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): " +
                                                    "CULLING waiting for despawn. " +
                                                    "Checking again in {2}sec",
                                                this.spawnPool.poolName,
                                                this.prefab.name,
                                                this.cullDelay));

                        break;
                    }
                }

                // Check again later
                yield return new WaitForSeconds(this.cullDelay);
            }

            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): CULLING FINISHED! Stopping",
                                        this.spawnPool.poolName,
                                        this.prefab.name));

            // Reset the singleton so the feature can be used again if needed.
            this.cullingActive = false;
            yield return null;

        }

        internal Transform SpawnInstance(Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            isNewInstance = false;
            if (this.limitInstances && this.limitFIFO && this._spawned.Count >= this.limitAmount)
            {
                Transform firstIn = this._spawned.First.Value;

                this.DespawnInstance(firstIn);
            }

            Transform inst;

            // If nothing is available, create a new instance
            if (this._despawned.Count == 0)
            {
                isNewInstance = true;
                // This will also handle limiting the number of NEW instances
                inst = this.SpawnNew(pos, rot);
            }
            else
            {
                // Switch the instance we are using to the spawned list
                // Use the first item in the list for ease
                inst = this._despawned.First.Value;
                this._despawned.RemoveFirst();
                this._spawned.AddLast(inst);

                // This came up for a user so this was added to throw a user-friendly error
                if (inst == null)
                {
                    var msg = "Make sure you didn't delete a despawned instance directly.";
                    throw new MissingReferenceException(msg);
                }

                inst.position = pos;
                inst.rotation = rot;
                if (SpawnType == SpawnType.Active) inst.gameObject.SetActive(true);

            }
            return inst;
        }


        /// <summary>
        /// 克隆新的对象
        /// </summary>
        public Transform SpawnNew(Vector3 pos, Quaternion rot)
        {
            // Handle limiting if the limit was used and reached.
            if (this.limitInstances && this.totalCount >= this.limitAmount)
            {
                YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format
                (
                    "SpawnPool {0} ({1}): " +
                            "LIMIT REACHED! Not creating new instances! (Returning null)",
                        this.spawnPool.poolName,
                        this.prefab.name
                ));

                return null;
            }

            // Use the SpawnPool group as the default position and rotation
            if (pos == Vector3.zero) pos = this.spawnPool.transform.position;
            if (rot == Quaternion.identity) rot = this.spawnPool.transform.rotation;

            GameObject instGO = InstanceHandler.InstantiatePrefab(prefabGO, pos, rot);
            Transform inst = instGO.transform;

            this.nameInstance(inst);  // Adds the number to the end

            // Start tracking the new instance
            this._spawned.AddLast(inst);

            YouYou.GameEntry.Log(YouYou.LogCategory.Pool, string.Format("SpawnPool {0} ({1}): Spawned new instance '{2}'.",
                                        this.spawnPool.poolName,
                                        this.prefab.name,
                                        inst.name));

            return inst;
        }


        #endregion Pool Functionality

        /// <summary>
        /// 对象名字后缀
        /// </summary>
        private void nameInstance(Transform instance)
        {
#if UNITY_EDITOR
            instance.name += (this.totalCount + 1).ToString("#000");
#endif
        }
    }
}
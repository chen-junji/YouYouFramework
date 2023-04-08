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
        #region Public Properties Available in the Editor
        /// <summary>
        /// 当预设池清空时候委托[改造]
        /// </summary>
        public System.Action<PrefabPool> OnPrefabPoolClear;

        /// <summary>
        /// The prefab to preload
        /// </summary>
        public Transform prefab;

        /// <summary>
        /// A reference of the prefab's GameObject stored for performance reasons
        /// </summary>
        public GameObject prefabGO;  // Hidden in inspector, but not Debug tab

        public SpawnType SpawnType;

        /// <summary>
        /// Limits the number of instances allowed in the game. Turning this ON
        ///	means when 'Limit Amount' is hit, no more instances will be created.
        /// CALLS TO SpawnPool.Spawn() WILL BE IGNORED, and return null!
        ///
        /// This can be good for non-critical objects like bullets or explosion
        ///	Flares. You would never want to use this for enemies unless it makes
        ///	sense to begin ignoring enemy spawns in the context of your game.
        /// </summary>
        public bool limitInstances = false;

        /// <summary>
        /// This is the max number of instances allowed if 'limitInstances' is ON.
        /// </summary>
        public int limitAmount = 100;

        /// <summary>
        /// FIFO stands for "first-in-first-out". Normally, limiting instances will
        /// stop spawning and return null. If this is turned on (set to true) the
        /// first spawned instance will be despawned and reused instead, keeping the
        /// total spawned instances limited but still spawning new instances.
        /// </summary>
        public bool limitFIFO = false;  // Keep after limitAmount for auto-inspector

        /// <summary>
        /// Turn this ON to activate the culling feature for this Pool. 
        /// Use this feature to remove despawned (inactive) instances from the pool
        /// if the size of the pool grows too large. 
        ///	
        /// DO NOT USE THIS UNLESS YOU NEED TO MANAGE MEMORY ISSUES!
        /// This should only be used in extreme cases for memory management. 
        /// For most pools (or games for that matter), it is better to leave this 
        /// off as memory is more plentiful than performance. If you do need this
        /// you can fine tune how often this is triggered to target extreme events.
        /// 
        /// A good example of when to use this would be if you you are Pooling 
        /// projectiles and usually never need more than 10 at a time, but then
        /// there is a big one-off fire-fight where 50 projectiles are needed. 
        /// Rather than keep the extra 40 around in memory from then on, set the 
        /// 'Cull Above' property to 15 (well above the expected max) and the Pool 
        /// will Destroy() the extra instances from the game to free up the memory. 
        /// 
        /// This won't be done immediately, because you wouldn't want this culling 
        /// feature to be fighting the Pool and causing extra Instantiate() and 
        /// Destroy() calls while the fire-fight is still going on. See 
        /// "Cull Delay" for more information about how to fine tune this.
        /// </summary>
        public bool cullDespawned = false;

        /// <summary>
        /// The number of TOTAL (spawned + despawned) instances to keep. 
        /// </summary>
        public int cullAbove = 50;

        /// <summary>
        /// The amount of time, in seconds, to wait before culling. This is timed 
        /// from the moment when the Queue's TOTAL count (spawned + despawned) 
        /// becomes greater than 'Cull Above'. Once triggered, the timer is repeated 
        /// until the count falls below 'Cull Above'.
        /// </summary>
        public int cullDelay = 60;

        /// <summary>
        /// The maximum number of instances to destroy per this.cullDelay
        /// </summary>
        public int cullMaxPerPass = 5;

        /// <summary>
        /// Prints information during run-time to make debugging easier. This will 
        /// be set to true if the owner SpawnPool is true, otherwise the user's setting
        /// here will be used
        /// </summary>
        public bool _logMessages = false;  // Used by the inspector
        public bool logMessages            // Read-only
        {
            get
            {
                if (forceLoggingSilent) return false;

                if (this.spawnPool.logMessages)
                    return this.spawnPool.logMessages;
                else
                    return this._logMessages;
            }
        }

        // Forces logging to be silent regardless of user settings.
        private bool forceLoggingSilent = false;


        /// <summary>
        /// Used internally to reference back to the owner spawnPool for things like
        /// anchoring co-routines.
        /// </summary>
        public SpawnPool spawnPool;
        #endregion Public Properties Available in the Editor


        #region Constructor and Self-Destruction
        /// <description>
        ///	Constructor to require a prefab Transform
        /// </description>
        public PrefabPool(Transform prefab)
        {
            this.prefab = prefab;
            this.prefabGO = prefab.gameObject;
        }

        /// <description>
        ///	Constructor for Serializable inspector use only
        /// </description>
        public PrefabPool() { }

        /// <description>
        ///	A pseudo constructor to init stuff not init by the serialized inspector-created
        ///	instance of this class.
        /// </description>
        internal void inspectorInstanceConstructor()
        {
            this.prefabGO = this.prefab.gameObject;
            this._spawned = new LinkedList<Transform>();
            this._despawned = new LinkedList<Transform>();
        }


        /// <summary>
        /// Run by a SpawnPool when it is destroyed
        /// </summary>
        internal void SelfDestruct()
        {
            if (this.logMessages)
                Debug.Log(string.Format(
                    "SpawnPool {0}: Cleaning up PrefabPool for {1}...", this.spawnPool.poolName, this.prefabGO.name
                ));

            // Go through both lists and destroy everything
            foreach (Transform inst in this._despawned)
                if (inst != null && this.spawnPool != null)  // Tear-down-time protection
                    this.spawnPool.DestroyInstance(inst.gameObject);

            foreach (Transform inst in this._spawned)
                if (inst != null && this.spawnPool != null)  // Tear-down-time protection
                    this.spawnPool.DestroyInstance(inst.gameObject);

            this._spawned.Clear();
            this._despawned.Clear();

            // Probably overkill but no harm done


            this.prefab = null;
            this.prefabGO = null;
            this.spawnPool = null;

        }
        #endregion Constructor and Self-Destruction


        #region Pool Functionality
        /// <summary>
        /// Is set to true when the culling coroutine is started so another one
        /// won't be
        /// </summary>
        private bool cullingActive = false;


        /// <summary>
        /// The active instances associated with this prefab. This is the pool of
        /// instances created by this prefab.
        /// 
        /// Managed by a SpawnPool
        /// </summary>
        internal LinkedList<Transform> _spawned = new LinkedList<Transform>();
        public LinkedList<Transform> spawned { get { return new LinkedList<Transform>(this._spawned); } }

        /// <summary>
        /// The deactive instances associated with this prefab. This is the pool of
        /// instances created by this prefab.
        /// 
        /// Managed by a SpawnPool
        /// </summary>
        internal LinkedList<Transform> _despawned = new LinkedList<Transform>();
        public LinkedList<Transform> despawned { get { return new LinkedList<Transform>(this._despawned); } }


        /// <summary>
        /// Returns the total count of instances in the PrefabPool
        /// </summary>
        public int totalCount
        {
            get
            {
                // Add all the items in the pool to get the total count
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
            this.spawnPool.DestroyInstance(xform.gameObject);

            //如果预设池里 没有物体了 从总池字典移除[改造]
            if (this.totalCount == 0)
            {
                this.spawnPool.prefabs.Remove(prefab.name);
                this.prefab = null;
                this.prefabGO = null;
                if (OnPrefabPoolClear != null)
                {
                    OnPrefabPoolClear(this);
                }
            }
        }


        /// <summary>
        /// Move an instance from despawned to spawned, set the position and 
        /// rotation, activate it and all children and return the transform
        /// </summary>
        /// <returns>
        /// True if successfull, false if xform isn't in the spawned list
        /// </returns>
        public bool DespawnInstance(Transform xform)
        {
            return DespawnInstance(xform, true);
        }

        public bool DespawnInstance(Transform xform, bool sendEventMessage)
        {
            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0} ({1}): Despawning '{2}'",
                                       this.spawnPool.poolName,
                                       this.prefab.name,
                                       xform.name));

            // Switch to the despawned list
            this._spawned.Remove(xform);
            this._despawned.AddLast(xform);

            // Notify instance of event OnDespawned for custom code additions.
            //   This is done before handling the deactivate and enqueue incase 
            //   there the user introduces an unforseen issue.
            if (sendEventMessage)
                xform.gameObject.BroadcastMessage(
                    "OnDespawned",
                    this.spawnPool,
                    SendMessageOptions.DontRequireReceiver
                );

            // Deactivate the instance and all children
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
        /// Waits for 'cullDelay' in seconds and culls the 'despawned' list if 
        /// above 'cullingAbove' amount. 
        /// 
        /// Triggered by DespawnInstance()
        /// </summary>
        internal IEnumerator CullDespawned()
        {
            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING TRIGGERED! " +
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
                        this.spawnPool.DestroyInstance(inst.gameObject);

                        if (this.logMessages)
                            Debug.Log(string.Format("SpawnPool {0} ({1}): " +
                                                    "CULLING to {2} instances. Now at {3}.",
                                                this.spawnPool.poolName,
                                                this.prefab.name,
                                                this.cullAbove,
                                                this.totalCount));

                        //如果预设池里 没有物体了 从总池字典移除[改造]
                        if (this.totalCount == 0)
                        {
                            this.spawnPool.prefabs.Remove(prefab.name);
                            this.prefab = null;
                            this.prefabGO = null;
                            if (OnPrefabPoolClear != null)
                            {
                                OnPrefabPoolClear(this);
                            }
                        }
                    }
                    else if (this.logMessages)
                    {
                        Debug.Log(string.Format("SpawnPool {0} ({1}): " +
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

            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0} ({1}): CULLING FINISHED! Stopping",
                                        this.spawnPool.poolName,
                                        this.prefab.name));

            // Reset the singleton so the feature can be used again if needed.
            this.cullingActive = false;
            yield return null;

        }

        /// <summary>
        /// 重新加入字典[改造]
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="prefab"></param>
        public void AddPrefabToDic(string prefabName, Transform prefab)
        {
            this.spawnPool.prefabs.Add(prefabName, prefab);
        }

        /// <summary>
        /// Move an instance from despawned to spawned, set the position and 
        /// rotation, activate it and all children and return the transform.
        /// 
        /// If there isn't an instance available, a new one is made.
        /// </summary>
        /// <returns>
        /// The new instance's Transform. 
        /// 
        /// If the Limit option was used for the PrefabPool associated with the
        /// passed prefab, then this method will return null if the limit is
        /// reached.
        /// </returns>    
        internal Transform SpawnInstance(Vector3 pos, Quaternion rot, ref bool isNewInstance)
        {
            isNewInstance = false;
            // Handle FIFO limiting if the limit was used and reached.
            //   If first-in-first-out, despawn item zero and continue on to respawn it
            if (this.limitInstances && this.limitFIFO &&
                this._spawned.Count >= this.limitAmount)
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


        public Transform SpawnNew()
        {
            return this.SpawnNew(Vector3.zero, Quaternion.identity);
        }
        public Transform SpawnNew(Vector3 pos, Quaternion rot)
        {
            // Handle limiting if the limit was used and reached.
            if (this.limitInstances && this.totalCount >= this.limitAmount)
            {
                if (this.logMessages)
                {
                    Debug.Log(string.Format
                    (
                        "SpawnPool {0} ({1}): " +
                                "LIMIT REACHED! Not creating new instances! (Returning null)",
                            this.spawnPool.poolName,
                            this.prefab.name
                    ));
                }

                return null;
            }

            // Use the SpawnPool group as the default position and rotation
            if (pos == Vector3.zero) pos = this.spawnPool.group.position;
            if (rot == Quaternion.identity) rot = this.spawnPool.group.rotation;

            GameObject instGO = this.spawnPool.InstantiatePrefab(this.prefabGO, pos, rot);
            Transform inst = instGO.transform;

            this.nameInstance(inst);  // Adds the number to the end

            if (!this.spawnPool.dontReparent)
            {
                // The SpawnPool group is the parent by default
                // This will handle RectTransforms as well
                var worldPositionStays = !(inst is RectTransform);
                inst.SetParent(this.spawnPool.group, worldPositionStays);
            }

            // Start tracking the new instance
            this._spawned.AddLast(inst);

            if (this.logMessages)
                Debug.Log(string.Format("SpawnPool {0} ({1}): Spawned new instance '{2}'.",
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
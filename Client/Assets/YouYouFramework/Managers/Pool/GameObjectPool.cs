//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// 游戏物体对象池
	/// </summary>
	public class GameObjectPool : IDisposable
	{
		/// <summary>
		/// 游戏物体对象池字典
		/// </summary>
		private Dictionary<byte, GameObjectPoolEntity> m_SpawnPoolDic;

		/// <summary>
		/// 实例ID对应对象池ID
		/// </summary>
		private Dictionary<int, byte> m_InstanceIdPoolIdDic;

		/// <summary>
		/// 空闲预设池队列 相当于对这个预设池再加了一层池
		/// </summary>
		private Queue<PrefabPool> m_PrefabPoolQueue;

		public GameObjectPool()
		{
			m_SpawnPoolDic = new Dictionary<byte, GameObjectPoolEntity>();
			m_InstanceIdPoolIdDic = new Dictionary<int, byte>();
			m_PrefabPoolQueue = new Queue<PrefabPool>();

			InstanceHandler.InstantiateDelegates += this.InstantiateDelegate;
			InstanceHandler.DestroyDelegates += this.DestroyDelegate;
		}

		public void Dispose()
		{
			m_SpawnPoolDic.Clear();
		}

		/// <summary>
		/// 当对象池物体创建时候
		/// </summary>
		/// <param name="prefab"></param>
		/// <param name="pos"></param>
		/// <param name="rot"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot, object userData)
		{
			ResourceEntity resourceEntity = userData as ResourceEntity;

			if (resourceEntity == null)
			{
				Debug.LogError("资源信息不存在 resourceEntity=" + resourceEntity.ResourceName);
				return null;
			}

			GameObject obj = UnityEngine.Object.Instantiate(prefab, pos, rot) as GameObject;

			//注册
			GameEntry.Pool.RegisterInstanceResource(obj.GetInstanceID(), resourceEntity);
			return obj;
		}

		/// <summary>
		/// 当对象池物体销毁时候
		/// </summary>
		/// <param name="instance"></param>
		public void DestroyDelegate(GameObject instance)
		{
			UnityEngine.Object.Destroy(instance);
			GameEntry.Resource.ResourceLoaderManager.UnLoadGameObject(instance);
		}

		#region Init 初始化
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public IEnumerator Init(GameObjectPoolEntity[] arr, Transform parent)
		{
			int len = arr.Length;
			for (int i = 0; i < len; i++)
			{
				GameObjectPoolEntity entity = arr[i];

				if (entity.Pool != null)
				{
					UnityEngine.Object.Destroy(entity.Pool.gameObject);
					yield return null;
					entity.Pool = null;
				}

				//创建对象池
				SpawnPool pool = PathologicalGames.PoolManager.Pools.Create(entity.PoolName);
				pool.group.parent = parent;
				pool.group.localPosition = Vector3.zero;
				entity.Pool = pool;

				m_SpawnPoolDic[entity.PoolId] = entity;
			}
		}
		#endregion

		#region Spawn 从对象池中获取对象
		private Dictionary<int, HashSet<Action<SpawnPool, Transform, ResourceEntity>>> m_LoadingPrefabPoolDic = new Dictionary<int, HashSet<Action<SpawnPool, Transform, ResourceEntity>>>();
		/// <summary>
		/// 从对象池中获取对象
		/// </summary>
		/// <param name="prefabId">预设编号</param>
		/// <param name="onComplete"></param>
		public void Spawn(Sys_PrefabEntity entity, BaseAction<Transform, bool> onComplete)
		{
			lock (m_PrefabPoolQueue)
			{
				//拿到对象池
				GameObjectPoolEntity gameObjectPoolEntity = m_SpawnPoolDic[(byte)entity.PoolId];

				//使用预设编号 当做池ID
				PrefabPool prefabPool = gameObjectPoolEntity.Pool.GetPrefabPool(entity.Id);
				if (prefabPool != null)
				{
					//拿到一个实例 激活一个已有的
					Transform retTrans = prefabPool.TrySpawnInstance();
					if (retTrans != null)
					{
						int instanceID = retTrans.gameObject.GetInstanceID();
						m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
						onComplete?.Invoke(retTrans, false);
						return;
					}
				}
				HashSet<Action<SpawnPool, Transform, ResourceEntity>> lst = null;
				if (m_LoadingPrefabPoolDic.TryGetValue(entity.Id, out lst))
				{
					//进行拦截
					//如果存在加载中的Asset 把委托加入对应的链表 然后直接返回
					lst.Add((_SpawnPool, _Transform, _ResourceEntity) =>
					{
						//拿到一个实例
						bool isNewInstance = false;
						Transform retTrans = _SpawnPool.Spawn(_Transform, ref isNewInstance, _ResourceEntity);
						int instanceID = retTrans.gameObject.GetInstanceID();
						m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
						onComplete?.Invoke(retTrans, isNewInstance);
					});
					return;
				}

				//这里说明是加载在第一个
				lst = GameEntry.Pool.DequeueClassObject<HashSet<Action<SpawnPool, Transform, ResourceEntity>>>();
				lst.Add((_SpawnPool, _Transform, _ResourceEntity) =>
				{
					//拿到一个实例
					bool isNewInstance = false;
					Transform retTrans = _SpawnPool.Spawn(_Transform, ref isNewInstance, _ResourceEntity);
					int instanceID = retTrans.gameObject.GetInstanceID();
					m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
					onComplete?.Invoke(retTrans, isNewInstance);
				});
				m_LoadingPrefabPoolDic[entity.Id] = lst;

				GameEntry.Resource.ResourceLoaderManager.LoadMainAsset((AssetCategory)entity.AssetCategory, entity.AssetFullName, (ResourceEntity resourceEntity) =>
				{
					GameObject retObj = resourceEntity.Target as GameObject;
					Transform prefab = retObj.transform;

					if (prefabPool == null)
					{
						//先去队列里找 空闲的池
						if (m_PrefabPoolQueue.Count > 0)
						{
							prefabPool = m_PrefabPoolQueue.Dequeue();

							prefabPool.PrefabPoolId = entity.Id; //设置预设池编号
							gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);

							prefabPool.prefab = prefab;
							prefabPool.prefabGO = prefab.gameObject;
							prefabPool.AddPrefabToDic(prefab.name, prefab);
						}
						else
						{
							prefabPool = new PrefabPool(prefab, entity.Id);
							gameObjectPoolEntity.Pool.CreatePrefabPool(prefabPool, resourceEntity);
						}

						prefabPool.OnPrefabPoolClear = (PrefabPool pool) =>
						{
							//预设池加入队列
							pool.PrefabPoolId = 0;
							gameObjectPoolEntity.Pool.RemovePrefabPool(pool);
							m_PrefabPoolQueue.Enqueue(pool);
						};

						//这些属性要从表格中读取
						prefabPool.cullDespawned = entity.CullDespawned == 1;
						prefabPool.cullAbove = entity.CullAbove;
						prefabPool.cullDelay = entity.CullDelay;
						prefabPool.cullMaxPerPass = entity.CullMaxPerPass;

					}
					var enumerator = lst.GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current?.Invoke(gameObjectPoolEntity.Pool, prefab, resourceEntity);
					}
					m_LoadingPrefabPoolDic.Remove(entity.Id);
					lst.Clear();//一定要清空
					GameEntry.Pool.EnqueueClassObject(lst);
				});
			}
		}
		#endregion

		#region Despawn 对象回池
		/// <summary>
		/// 对象回池
		/// </summary>
		/// <param name="poolId"></param>
		/// <param name="instance">实例</param>
		internal void Despawn(byte poolId, Transform instance)
		{
			GameObjectPoolEntity entity = m_SpawnPoolDic[poolId];
			instance.SetParent(entity.Pool.transform);
			entity.Pool.Despawn(instance);
		}

		/// <summary>
		/// 对象回池
		/// </summary>
		/// <param name="instance">实例</param>
		public void Despawn(Transform instance)
		{
			int instanceID = instance.gameObject.GetInstanceID();
			byte poolId = m_InstanceIdPoolIdDic[instanceID];
			m_InstanceIdPoolIdDic.Remove(instanceID);
			Despawn(poolId, instance);
		}
		#endregion
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYou
{
	/// <summary>
	/// 池管理器
	/// </summary>
	public class PoolManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// 类对象池
		/// </summary>
		public ClassObjectPool ClassObjectPool { get; private set; }
		/// <summary>
		/// 游戏物体对象池
		/// </summary>
		private GameObjectPool GameObjectPool;
		/// <summary>
		/// 资源包池
		/// </summary>
		public ResourcePool AssetBundlePool { get; private set; }
		/// <summary>
		/// 分类资源池
		/// </summary>
		public Dictionary<AssetCategory, ResourcePool> AssetPool { get; private set; }

		internal PoolManager()
		{
			ClassObjectPool = new ClassObjectPool();
			GameObjectPool = new GameObjectPool();

			AssetBundlePool = new ResourcePool("AssetBundlePool");
			m_InstanceResourceDic = new Dictionary<int, ResourceEntity>();
			AssetPool = new Dictionary<AssetCategory, ResourcePool>();
		}

		/// <summary>
		/// 初始化
		/// </summary>
		internal override void Init()
		{
			ReleaseClassObjectInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseClassObjectInterval, GameEntry.CurrDeviceGrade);
			ReleaseAssetBundleInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetBundleInterval, GameEntry.CurrDeviceGrade);
			ReleaseAssetInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetInterval, GameEntry.CurrDeviceGrade);

			//确保游戏刚开始运行的时候 分类资源池已经初始化好了
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				AssetPool[assetCategory] = new ResourcePool(assetCategory.ToString());
			}

			ReleaseClassObjectNextRunTime = Time.time;
			ReleaseAssetBundleNextRunTime = Time.time;
			ReleaseAssetNextRunTime = Time.time;

			InitGameObjectPool();
			m_LockedAssetBundleLength = GameEntry.Instance.LockedAssetBundle.Length;
			InitClassReside();
		}

		/// <summary>
		/// 释放类对象池
		/// </summary>
		public void ReleaseClassObjectPool()
		{
			ClassObjectPool.Release();
		}

		/// <summary>
		/// 释放资源包池
		/// </summary>
		public void ReleaseAssetBundlePool()
		{
			AssetBundlePool.Release();
		}

		/// <summary>
		/// 释放分类资源池中所有资源
		/// </summary>
		public void ReleaseAssetPool()
		{
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				AssetPool[assetCategory].Release();
			}
		}

		public void Dispose()
		{
			ClassObjectPool.Dispose();
			GameObjectPool.Dispose();
		}

		//============================


		/// <summary>
		/// 锁定的资源包数组长度
		/// </summary>
		private int m_LockedAssetBundleLength;

		/// <summary>
		/// 检查资源包是否锁定
		/// </summary>
		/// <param name="assetBundleName">资源包名称</param>
		/// <returns></returns>
		public bool CheckAssetBundleIsLock(string assetBundleName)
		{
			for (int i = 0; i < m_LockedAssetBundleLength; i++)
			{
				if (GameEntry.Instance.LockedAssetBundle[i].Equals(assetBundleName, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 初始化常用类常驻数量
		/// </summary>
		private void InitClassReside()
		{
			SetClassObjectResideCount<HttpRoutine>(3);
			SetClassObjectResideCount<Dictionary<string, object>>(3);
			SetClassObjectResideCount<AssetBundleLoaderRoutine>(10);
			SetClassObjectResideCount<AssetLoaderRoutine>(10);
			SetClassObjectResideCount<ResourceEntity>(10);
			SetClassObjectResideCount<MainAssetLoaderRoutine>(30);
		}
		/// <summary>
		/// 设置类常驻数量
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count"></param>
		public void SetClassObjectResideCount<T>(byte count) where T : class
		{
			ClassObjectPool.SetResideCount<T>(count);
		}

		#region DequeueClassObject 取出一个对象
		/// <summary>
		/// 取出一个对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T DequeueClassObject<T>() where T : class, new()
		{
			return ClassObjectPool.Dequeue<T>();
		}
		#endregion

		#region EnqueueClassObject 对象回池
		/// <summary>
		/// 对象回池
		/// </summary>
		/// <param name="obj"></param>
		public void EnqueueClassObject(object obj)
		{
			ClassObjectPool.Enqueue(obj);
		}
		#endregion

		#region 变量对象池

		/// <summary>
		/// 变量对象池锁
		/// </summary>
		private object m_VarObjectLock = new object();

#if UNITY_EDITOR
		/// <summary>
		/// 在监视面板显示的信息
		/// </summary>
		public Dictionary<Type, int> VarObjectInspectorDic = new Dictionary<Type, int>();
#endif

		/// <summary>
		/// 取出一个变量对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T DequeueVarObject<T>() where T : VariableBase, new()
		{
			lock (m_VarObjectLock)
			{
				T item = ClassObjectPool.Dequeue<T>();
#if UNITY_EDITOR
				Type t = item.GetType();
				if (VarObjectInspectorDic.ContainsKey(t))
				{
					VarObjectInspectorDic[t]++;
				}
				else
				{
					VarObjectInspectorDic[t] = 1;
				}
#endif
				return item;
			}
		}

		/// <summary>
		/// 变量对象回池
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		public void EnqueueVarObject<T>(T item) where T : VariableBase
		{
			lock (m_VarObjectLock)
			{
				ClassObjectPool.Enqueue(item);
#if UNITY_EDITOR
				Type t = item.GetType();
				if (VarObjectInspectorDic.ContainsKey(t))
				{
					VarObjectInspectorDic[t]--;
					if (VarObjectInspectorDic[t] == 0)
					{
						VarObjectInspectorDic.Remove(t);
					}
				}
#endif
			}
		}

		#endregion

		/// <summary>
		/// 释放类对象池间隔
		/// </summary>
		public int ReleaseClassObjectInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// 下次释放类对象运行时间
		/// </summary>
		public float ReleaseClassObjectNextRunTime
		{
			get;
			private set;
		}


		/// <summary>
		/// 释放AssetBundle池间隔
		/// </summary>
		public int ReleaseAssetBundleInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// 下次释放AssetBundle池运行时间
		/// </summary>
		public float ReleaseAssetBundleNextRunTime
		{
			get;
			private set;
		}

		/// <summary>
		/// 释放Asset池间隔
		/// </summary>
		public int ReleaseAssetInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// 下次释放Asset池运行时间
		/// </summary>
		public float ReleaseAssetNextRunTime
		{
			get;
			private set;
		}

		internal void OnUpdate()
		{
			if (Time.time > ReleaseClassObjectNextRunTime + ReleaseClassObjectInterval)
			{
				ReleaseClassObjectNextRunTime = Time.time;
				ReleaseClassObjectPool();
				GameEntry.Log(LogCategory.Normal, "释放类对象池");
			}


			if (Time.time > ReleaseAssetBundleNextRunTime + ReleaseAssetBundleInterval)
			{
				ReleaseAssetBundleNextRunTime = Time.time;

#if !EDITORLOAD
				ReleaseAssetBundlePool();
				GameEntry.Log(LogCategory.Normal, "释放AssetBundle池");
#endif
			}

			if (Time.time > ReleaseAssetNextRunTime + ReleaseAssetInterval)
			{
				ReleaseAssetNextRunTime = Time.time;

#if !EDITORLOAD
				ReleaseAssetPool();
				GameEntry.Log(LogCategory.Normal, "释放Asset池");
#endif
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.LuaFullGc);

#if !UNLOADRES_CHANGESCENE
				if (LuaManager.luaEnv != null)
				{
					LuaManager.luaEnv.FullGc();
				}
				Resources.UnloadUnusedAssets();
#endif
			}
		}

		#region 游戏物体对象池

		/// <summary>
		/// 初始化游戏物体对象池
		/// </summary>
		private void InitGameObjectPool()
		{
			GameEntry.Instance.StartCoroutine(GameObjectPool.Init(GameEntry.Instance.GameObjectPoolGroups, GameEntry.Instance.PoolParent));
		}

		/// <summary>
		/// 从对象池中获取对象
		/// </summary>
		/// <param name="prefabId">预设编号</param>
		/// <param name="onComplete"></param>
		public void GameObjectSpawn(int prefabId, BaseAction<Transform, bool> onComplete)
		{
			GameObjectSpawn(GameEntry.DataTable.Sys_PrefabDBModel.GetDic(prefabId), onComplete);
		}
		public void GameObjectSpawn(string prefabName, BaseAction<Transform, bool> onComplete)
		{
			GameObjectSpawn(GameEntry.DataTable.Sys_PrefabDBModel.GetPrefabIdByName(prefabName), onComplete);
		}
		public void GameObjectSpawn(Sys_PrefabEntity sys_PrefabEntity, BaseAction<Transform, bool> onComplete)
		{
			if (sys_PrefabEntity == null)
			{
				GameEntry.LogError("预设数据不存在,sys_PrefabEntity==null!");
				return;
			}
			GameObjectPool.Spawn(sys_PrefabEntity, onComplete);
		}

		/// <summary>
		/// 对象回池
		/// </summary>
		/// <param name="instance">实例</param>
		public void GameObjectDespawn(Transform instance)
		{
			GameObjectPool.Despawn(instance);
		}
		#endregion

		#region 实例管理和分类资源池释放
		/// <summary>
		/// 克隆出来的实例资源字典
		/// </summary>
		private Dictionary<int, ResourceEntity> m_InstanceResourceDic;

		/// <summary>
		/// 注册到实例字典
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="resourceEntity"></param>
		public void RegisterInstanceResource(int instanceId, ResourceEntity resourceEntity)
		{
			//Debug.LogError("注册到实例字典instanceId=" + instanceId);
			m_InstanceResourceDic[instanceId] = resourceEntity;
		}

		/// <summary>
		/// 释放实例资源
		/// </summary>
		/// <param name="instanceId"></param>
		public void ReleaseInstanceResource(int instanceId)
		{
			//Debug.LogError("释放实例资源instanceId=" + instanceId);
			ResourceEntity resourceEntity = null;
			if (m_InstanceResourceDic.TryGetValue(instanceId, out resourceEntity))
			{
#if EDITORLOAD
				resourceEntity.Target = null;
				GameEntry.Pool.EnqueueClassObject(resourceEntity);
#else
				UnspawnResourceEntity(resourceEntity);
#endif
				m_InstanceResourceDic.Remove(instanceId);
			}
		}

		/// <summary>
		/// 资源实体回池
		/// </summary>
		/// <param name="entity"></param>
		private void UnspawnResourceEntity(ResourceEntity entity)
		{
			var curr = entity.DependsResourceList.First;
			while (curr != null)
			{
				UnspawnResourceEntity(curr.Value);
				curr = curr.Next;
			}

			AssetPool[entity.Category].Unspawn(entity.ResourceName);
		}
		#endregion
	}
}
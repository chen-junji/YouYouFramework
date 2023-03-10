using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// 资源池
	/// </summary>
	public class ResourcePool
	{
#if UNITY_EDITOR
		/// <summary>
		/// 在监视面板显示的信息
		/// </summary>
		public Dictionary<string, ResourceEntity> InspectorDic = new Dictionary<string, ResourceEntity>();
#endif

		/// <summary>
		/// 资源池名称
		/// </summary>
		public string PoolName { get; private set; }

		/// <summary>
		/// 资源池字典
		/// </summary>
		private Dictionary<string, ResourceEntity> m_ResourceDic;

		/// <summary>
		/// 需要移除的Key链表
		/// </summary>
		private LinkedList<string> m_NeedRemoveKeyList;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="poolName">资源池名称</param>
		public ResourcePool(string poolName)
		{
			PoolName = poolName;
			m_ResourceDic = new Dictionary<string, ResourceEntity>();
			m_NeedRemoveKeyList = new LinkedList<string>();
		}

		/// <summary>
		/// 注册到资源池(引用计数+1)
		/// </summary>
		/// <param name="entity"></param>
		public void Register(ResourceEntity entity, bool reference = true)
		{
			entity.Spawn(reference);
#if UNITY_EDITOR
			//if (!InspectorDic.ContainsKey(entity.ResourceName)) InspectorDic.Add(entity.ResourceName, entity);
			InspectorDic.Add(entity.ResourceName, entity);
#endif
			//if (!m_ResourceDic.ContainsKey(entity.ResourceName)) m_ResourceDic.Add(entity.ResourceName, entity);
			m_ResourceDic.Add(entity.ResourceName, entity);
		}

		/// <summary>
		/// 资源取池(引用计数+1)
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		public ResourceEntity Spawn(string resourceName, bool reference = true)
		{
			ResourceEntity resourceEntity = null;
			if (m_ResourceDic.TryGetValue(resourceName, out resourceEntity))
			{
				resourceEntity.Spawn(reference);
#if UNITY_EDITOR
				if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
				{
					InspectorDic[resourceEntity.ResourceName] = resourceEntity;
				}
#endif
			}
			return resourceEntity;
		}

		/// <summary>
		/// 资源回池
		/// </summary>
		/// <param name="resourceName"></param>
		public void Unspawn(string resourceName)
		{
			ResourceEntity resourceEntity = null;
			if (m_ResourceDic.TryGetValue(resourceName, out resourceEntity))
			{
				resourceEntity.Unspawn();
#if UNITY_EDITOR
				if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
				{
					InspectorDic[resourceEntity.ResourceName] = resourceEntity;
				}
#endif
			}
		}

		/// <summary>
		/// 释放资源池中可释放资源
		/// </summary>
		public void Release()
		{
			var enumerator = m_ResourceDic.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ResourceEntity resourceEntity = enumerator.Current.Value;
				if (resourceEntity.GetCanRelease())
				{
#if UNITY_EDITOR
					if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
					{
						InspectorDic.Remove(resourceEntity.ResourceName);
					}
#endif
					m_NeedRemoveKeyList.AddFirst(resourceEntity.ResourceName);
					resourceEntity.Release();
				}
			}

			//循环链表 从字典中移除制定的Key
			LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
			while (curr != null)
			{
				string key = curr.Value;
				m_ResourceDic.Remove(key);

				LinkedListNode<string> next = curr.Next;
				m_NeedRemoveKeyList.Remove(curr);
				curr = next;
			}
		}

		/// <summary>
		/// 释放池内所有资源
		/// </summary>
		public void ReleaseAll()
		{
			var enumerator = m_ResourceDic.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ResourceEntity resourceEntity = enumerator.Current.Value;
#if UNITY_EDITOR
				if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
				{
					InspectorDic.Remove(resourceEntity.ResourceName);
				}
#endif
				m_NeedRemoveKeyList.AddFirst(resourceEntity.ResourceName);
				resourceEntity.Release();
			}

			//循环链表 从字典中移除制定的Key
			LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
			while (curr != null)
			{
				string key = curr.Value;
				m_ResourceDic.Remove(key);

				LinkedListNode<string> next = curr.Next;
				m_NeedRemoveKeyList.Remove(curr);
				curr = next;
			}
		}
	}
}
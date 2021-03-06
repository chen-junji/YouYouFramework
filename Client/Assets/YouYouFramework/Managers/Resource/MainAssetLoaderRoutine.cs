using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// 主资源加载器
	/// </summary>
	public class MainAssetLoaderRoutine
	{
		/// <summary>
		/// 当前的资源信息
		/// </summary>
		private AssetEntity m_CurrAssetEnity;

		/// <summary>
		/// 当前的资源实体
		/// </summary>
		private ResourceEntity m_CurrResourceEntity;

		/// <summary>
		/// 当前资源的依赖实体链表(临时存储)
		/// </summary>
		private LinkedList<ResourceEntity> m_DependResourceList = new LinkedList<ResourceEntity>();

		/// <summary>
		/// 需要加载的依赖资源数量
		/// </summary>
		private int m_NeedLoadAssetDependCount = 0;

		/// <summary>
		/// 当前已经加载的依赖资源数量
		/// </summary>
		private int m_CurrLoadAssetDependCount = 0;

		/// <summary>
		/// 当前主资源加载器 加载完毕
		/// </summary>
		private BaseAction<ResourceEntity> m_OnComplete;

		/// <summary>
		/// 是否递增引用计数
		/// </summary>
		private bool m_IsAddReferenceCount;

		/// <summary>
		/// 主资源或依赖资源
		/// </summary>
		private bool m_MainOrDepends;

		/// <summary>
		/// 加载主资源
		/// </summary>
		/// <param name="assetCategory"></param>
		/// <param name="assetFullName"></param>
		/// <param name="onComplete"></param>
		internal void Load(AssetCategory assetCategory, string assetFullName, bool isAddReferenceCount, bool mainOrDepends, BaseAction<ResourceEntity> onComplete)
		{
			m_IsAddReferenceCount = isAddReferenceCount;
			m_MainOrDepends = mainOrDepends;
#if EDITORLOAD && UNITY_EDITOR
			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.Category = assetCategory;
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFullName);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#elif RESOURCES
			string resourcesPath = assetFullName.Split('.')[0].Replace("Assets/Download/", string.Empty);

			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.Category = assetCategory;
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = Resources.Load(resourcesPath);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#else
			m_OnComplete = onComplete;
			m_CurrAssetEnity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetCategory, assetFullName);
			if (m_CurrAssetEnity != null) LoadDependsAsset();
#endif
		}

		/// <summary>
		/// 加载当前资源
		/// </summary>
		private void LoadCurrAsset()
		{
			//第一步. 加载Assetbundle
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(m_CurrAssetEnity.AssetBundleName, onComplete: (ResourceEntity bundleEntity) =>
			{
				if (!m_MainOrDepends)
				{
					m_OnComplete?.Invoke(null);
					return;
				}
				//第二步. 从分类资源池(AssetPool)中查找Asset
				m_CurrResourceEntity = GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Spawn(m_CurrAssetEnity.AssetFullName, m_IsAddReferenceCount);
				if (m_CurrResourceEntity != null)
				{
					//GameEntry.Log(LogCategory.Resource, "从分类资源池中加载{0}=>{1}", m_CurrResourceEntity.Target, m_CurrResourceEntity.ResourceName);
					m_OnComplete?.Invoke(m_CurrResourceEntity);
					return;
				}

				//如果池中没有, 那么加载Asset
				GameEntry.Resource.ResourceLoaderManager.LoadAsset(m_CurrAssetEnity.Category, m_CurrAssetEnity.AssetFullName, bundleEntity.Target as AssetBundle, onComplete: (UnityEngine.Object obj, bool isNew) =>
				{
					//LoadAsset有高并发,这里处理了ResourceEntity多次Register的情况
					m_CurrResourceEntity = GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Spawn(m_CurrAssetEnity.AssetFullName, m_IsAddReferenceCount);
					if (m_CurrResourceEntity != null)
					{
						m_OnComplete?.Invoke(m_CurrResourceEntity);
						return;
					}

					//初始化ResourceEntity,并注册到资源池
					m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
					m_CurrResourceEntity.Category = m_CurrAssetEnity.Category;
					m_CurrResourceEntity.IsAssetBundle = false;
					m_CurrResourceEntity.ResourceName = m_CurrAssetEnity.AssetFullName;
					m_CurrResourceEntity.Target = obj;
					GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Register(m_CurrResourceEntity, m_IsAddReferenceCount);

					//加入到这个资源的依赖资源链表里
					//var currDependsResource = m_DependResourceList.First;
					//while (currDependsResource != null)
					//{
					//	var next = currDependsResource.Next;
					//	m_DependResourceList.Remove(currDependsResource);
					//	m_CurrResourceEntity.DependsResourceList.AddLast(currDependsResource);
					//	currDependsResource = next;
					//}

					//当前主资源加载器 加载完毕(类递归)
					m_OnComplete?.Invoke(m_CurrResourceEntity);

					Reset();
				});
			});
		}

		/// <summary>
		/// 加载依赖资源
		/// </summary>
		private void LoadDependsAsset()
		{
			List<AssetDependsEntity> lst = m_CurrAssetEnity.DependsAssetList;
			if (lst != null)
			{
				int len = lst.Count;
				m_NeedLoadAssetDependCount = len;
				for (int i = 0; i < len; i++)
				{
					AssetDependsEntity entity = lst[i];
					MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
					routine.Load(entity.Category, entity.AssetFullName, m_IsAddReferenceCount, false, (ResourceEntity res) =>
					{
						//把这个主资源依赖的资源实体 加入临时链表
						//m_DependResourceList.AddLast(res);

						//把加载出来的资源 加入到池 需要做
						m_CurrLoadAssetDependCount++;

						//依赖加载完毕了, 加载当前资源
						if (m_NeedLoadAssetDependCount == m_CurrLoadAssetDependCount) LoadCurrAsset();
					});
				}
			}
			else
			{
				//没有依赖 直接加载当前资源
				LoadCurrAsset();
			}
		}

		/// <summary>
		/// 重置
		/// </summary>
		private void Reset()
		{
			m_OnComplete = null;
			m_CurrAssetEnity = null;
			m_CurrResourceEntity = null;
			m_NeedLoadAssetDependCount = 0;
			m_CurrLoadAssetDependCount = 0;
			m_DependResourceList.Clear();
			m_IsAddReferenceCount = false;
			m_MainOrDepends = false;
			GameEntry.Pool.EnqueueClassObject(this);
		}

	}
}
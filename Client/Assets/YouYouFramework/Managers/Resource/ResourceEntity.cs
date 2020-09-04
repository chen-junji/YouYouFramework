using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// 资源实体(AssetBundle和Asset)
	/// </summary>
	public class ResourceEntity
	{
		/// <summary>
		/// 资源名称
		/// </summary>
		public string ResourceName;

		/// <summary>
		/// 资源分类(用于Asset)
		/// </summary>
		public AssetCategory Category;

		/// <summary>
		/// 是否AssetBundle
		/// </summary>
		public bool IsAssetBundle;

		/// <summary>
		/// 关联目标
		/// </summary>
		public object Target;

		/// <summary>
		/// 上次使用时间
		/// </summary>
		public float LastUseTime { get; private set; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int ReferenceCount { get; private set; }

		/// <summary>
		/// 依赖的资源实体链表
		/// </summary>
		public LinkedList<ResourceEntity> DependsResourceList { get; private set; }

		public ResourceEntity()
		{
			DependsResourceList = new LinkedList<ResourceEntity>();
		}

		/// <summary>
		/// 对象取池
		/// </summary>
		public void Spawn()
		{
			LastUseTime = Time.time;

			if (!IsAssetBundle)
			{
				ReferenceCount++;
			}
			else
			{
				//如果是锁定的资源包 不释放
				if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
				{
					ReferenceCount = 1;
				}
			}
		}

		/// <summary>
		/// 对象回池
		/// </summary>
		public void Unspawn()
		{
			LastUseTime = Time.time;

			ReferenceCount--;
			if (ReferenceCount < 0)
			{
				ReferenceCount = 0;
			}
		}

		/// <summary>
		/// 对象是否可以释放
		/// </summary>
		/// <returns></returns>
		public bool GetCanRelease()
		{
			if (ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Release()
		{
			ResourceName = null;
			ReferenceCount = 0;

			if (IsAssetBundle)
			{
				AssetBundle bundle = Target as AssetBundle;
				//GameEntry.Log(LogCategory.Resource, "卸载了资源包=>{0}", bundle.name);
				bundle.Unload(false);
			}
			Target = null;

			DependsResourceList.Clear();//把依赖的资源实体链表清空
			GameEntry.Pool.EnqueueClassObject(this);//当前资源实体回池
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYou
{
	/// <summary>
	/// 资源加载管理器
	/// </summary>
	public class ResourceLoaderManager : IDisposable
	{
		/// <summary>
		/// 资源信息字典
		/// </summary>
		private Dictionary<AssetCategory, Dictionary<string, AssetEntity>> m_AssetInfoDic;

		/// <summary>
		/// 资源包加载器链表
		/// </summary>
		private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

		/// <summary>
		/// 资源加载器链表
		/// </summary>
		private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;

		public ResourceLoaderManager()
		{
			m_AssetInfoDic = new Dictionary<AssetCategory, Dictionary<string, AssetEntity>>();

			//确保游戏刚开始运行的时候 分类字典已经初始化好了
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				m_AssetInfoDic[assetCategory] = new Dictionary<string, AssetEntity>();
			}

			m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
			m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();
		}
		internal void OnUpdate()
		{
			for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
			{
				curr.Value.OnUpdate();
			}

			for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
			{
				curr.Value.OnUpdate();
			}
		}

		internal void Init()
		{
		}

		public void Dispose()
		{
			m_AssetInfoDic.Clear();
			m_AssetLoaderList.Clear();
		}

		/// <summary>
		/// 获取资源信息实体
		/// </summary>
		/// <param name="assetCategory">资源分类</param>
		/// <param name="assetFullName">资源路径</param>
		/// <returns></returns>
		internal AssetEntity GetAssetEntity(AssetCategory assetCategory, string assetFullName)
		{
			Dictionary<string, AssetEntity> dicCategory = null;
			if (m_AssetInfoDic.TryGetValue(assetCategory, out dicCategory))
			{
				AssetEntity entity = null;
				if (dicCategory.TryGetValue(assetFullName, out entity))
				{
					return entity;
				}
			}
			GameEntry.LogError("assetCategory=>{0}, assetFullName=>{1}不存在", assetCategory, assetFullName);
			return null;
		}

		/// <summary>
		/// 加载主资源
		/// </summary>
		/// <param name="assetCategory"></param>
		/// <param name="assetFullName"></param>
		/// <param name="onComplete"></param>
		public void LoadMainAsset<T>(AssetCategory assetCategory, string assetFullName, BaseAction<T> onComplete)
		{
			LoadMainAsset(assetCategory, assetFullName, (ResourceEntity resEntity) =>
			{
				onComplete?.Invoke((T)resEntity.Target);
			});
		}
		public void LoadMainAsset(AssetCategory assetCategory, string assetFullName, BaseAction<ResourceEntity> onComplete)
		{
			MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
			routine.Load(assetCategory, assetFullName, (ResourceEntity resEntity) =>
			{
				if (resEntity.Target != null)
				{
					onComplete?.Invoke(resEntity);
				}
				else
				{
					GameEntry.LogError("资源加载失败! assetFullName==" + assetFullName);
				}
			});
		}

		public void UnLoadGameObject(GameObject obj)
		{
			GameEntry.Pool.ReleaseInstanceResource(obj.GetInstanceID());
		}

		#region InitAssetInfo 初始化资源信息
		private BaseAction m_InitAssetInfoComplete;
		/// <summary>
		/// 初始化资源信息
		/// </summary>
		internal void InitAssetInfo(BaseAction initAssetInfoComplete)
		{
			m_InitAssetInfoComplete = initAssetInfoComplete;

			byte[] buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(YFConstDefine.AssetInfoName);
			if (buffer == null)
			{
				//如果可写区没有 那么就从只读区获取
				GameEntry.Resource.ResourceManager.StreamingAssetsManager.ReadAssetBundle(YFConstDefine.AssetInfoName, (byte[] buff) =>
				 {
					 if (buff == null)
					 {
						 //如果只读区也没有,从CDN读取
						 string url = string.Format("{0}{1}", GameEntry.Data.SysDataManager.CurrChannelConfig.RealSourceUrl, YFConstDefine.AssetInfoName);
						 GameEntry.Http.Get(url, (HttpCallBackArgs args) =>
						 {
							 GameEntry.Log(LogCategory.Normal, "从CDN初始化资源信息");
							 InitAssetInfo(args.Data);
						 });
					 }
					 else
					 {
						 GameEntry.Log(LogCategory.Normal, "从只读区初始化资源信息");
						 InitAssetInfo(buff);
					 }
				 });
			}
			else
			{
				GameEntry.Log(LogCategory.Normal, "从可写区初始化资源信息");
				InitAssetInfo(buffer);
			}
		}


		/// <summary>
		/// 初始化资源信息
		/// </summary>
		/// <param name="buffer"></param>
		private void InitAssetInfo(byte[] buffer)
		{
			buffer = ZlibHelper.DeCompressBytes(buffer);//解压

			MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
			int len = ms.ReadInt();
			int depLen = 0;
			for (int i = 0; i < len; i++)
			{
				AssetEntity entity = new AssetEntity();
				entity.Category = (AssetCategory)ms.ReadByte();
				entity.AssetFullName = ms.ReadUTF8String();
				entity.AssetBundleName = ms.ReadUTF8String();

				//Debug.Log("entity.Category=" + entity.Category);
				//Debug.Log("entity.AssetBundleName=" + entity.AssetBundleName);

				depLen = ms.ReadInt();
				if (depLen > 0)
				{
					entity.DependsAssetList = new List<AssetDependsEntity>(depLen);
					for (int j = 0; j < depLen; j++)
					{
						AssetDependsEntity assetDepends = new AssetDependsEntity();
						assetDepends.Category = (AssetCategory)ms.ReadByte();
						assetDepends.AssetFullName = ms.ReadUTF8String();
						entity.DependsAssetList.Add(assetDepends);
					}
				}

				m_AssetInfoDic[entity.Category][entity.AssetFullName] = entity;
			}

			m_InitAssetInfoComplete?.Invoke();
		}
		#endregion

		#region LoadAssetBundle 加载资源包
		/// <summary>
		/// 加载中的Bundle
		/// </summary>
		private Dictionary<string, LinkedList<Action<AssetBundle>>> m_LoadingAssetBundle = new Dictionary<string, LinkedList<Action<AssetBundle>>>();

		/// <summary>
		/// 加载资源包
		/// </summary>
		/// <param name="assetbundlePath"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAssetBundle(string assetbundlePath, Action<float> onUpdate = null, Action<AssetBundle> onComplete = null)
		{
			//Debug.LogError("加载资源包" + assetbundlePath);
			//1.判断资源包是否存在于AssetBundlePool
			ResourceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
			if (assetBundleEntity != null)
			{
				//Debug.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
				AssetBundle assetBundle = assetBundleEntity.Target as AssetBundle;
				if (onComplete != null) onComplete(assetBundle);
				return;
			}

			//2.判断Bundle是否加载到一半,防止高并发导致重复加载
			LinkedList<Action<AssetBundle>> lst = null;
			if (m_LoadingAssetBundle.TryGetValue(assetbundlePath, out lst))
			{
				//如果Bundle已经在加载中, 把委托加入对应的链表 然后直接return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//如果Bundle还没有开始加载, 把委托加入对应的链表 然后开始加载
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<AssetBundle>>>();
				lst.AddLast(onComplete);
				m_LoadingAssetBundle[assetbundlePath] = lst;
			}


			AssetBundleLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetBundleLoaderRoutine>();
			if (routine == null) routine = new AssetBundleLoaderRoutine();

			//加入链表开始循环
			m_AssetBundleLoaderList.AddLast(routine);

			//加载资源包
			routine.LoadAssetBundle(assetbundlePath);
			//资源包加载 进行中 回调
			routine.OnAssetBundleCreateUpdate = onUpdate;
			//资源包加载 结束 回调
			routine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
			{
				//资源包取池
				assetBundleEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
				assetBundleEntity.ResourceName = assetbundlePath;
				assetBundleEntity.IsAssetBundle = true;
				assetBundleEntity.Target = assetbundle;
				//资源包注册到资源池
				GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);

				for (LinkedListNode<Action<AssetBundle>> curr = lst.First; curr != null; curr = curr.Next)
				{
					if (curr.Value != null) curr.Value(assetbundle);
				}
				//资源加载完毕后
				lst.Clear();//必须清空
				GameEntry.Pool.EnqueueClassObject(lst);
				m_LoadingAssetBundle.Remove(assetbundlePath);//从加载中的Bundle的Dic 移除

				//结束循环 回池
				m_AssetBundleLoaderList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
			};
		}
		#endregion

		#region LoadAsset 从资源包中加载资源
		/// <summary>
		/// 加载中的Asset
		/// </summary>
		private Dictionary<string, LinkedList<Action<UnityEngine.Object>>> m_LoadingAsset = new Dictionary<string, LinkedList<Action<UnityEngine.Object>>>();
		/// <summary>
		/// 从资源包中加载资源
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetBundle"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAsset(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null, Action<UnityEngine.Object> onComplete = null)
		{
			//Debug.Log(assetName + "===========================================================");
			//1.判断Asset是否加载到一半,防止高并发导致重复加载
			LinkedList<Action<UnityEngine.Object>> lst = null;
			if (m_LoadingAsset.TryGetValue(assetName, out lst))
			{
				//如果Asset已经在加载中, 把委托加入对应的链表 然后直接return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//如果Asset还没有开始加载, 把委托加入对应的链表 然后开始加载
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<UnityEngine.Object>>>();
				lst.AddLast(onComplete);
				m_LoadingAsset[assetName] = lst;
			}


			AssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetLoaderRoutine>();
			if (routine == null) routine = new AssetLoaderRoutine();

			//加入链表开始循环
			m_AssetLoaderList.AddLast(routine);

			//加载资源
			routine.LoadAsset(assetName, assetBundle);
			//资源加载 进行中 回调
			routine.OnAssetUpdate = (float progress) =>
			{
				if (onUpdate != null) onUpdate(progress);
			};
			//资源加载 结果 回调
			routine.OnLoadAssetComplete = (UnityEngine.Object obj) =>
			{
				for (LinkedListNode<Action<UnityEngine.Object>> curr = lst.First; curr != null; curr = curr.Next)
				{
					if (curr.Value != null) curr.Value(obj);
				}
				//资源加载完毕后
				lst.Clear();//必须清空
				GameEntry.Pool.EnqueueClassObject(lst);
				m_LoadingAsset.Remove(assetName);//从加载中的Asset的Dic 移除

				//结束循环 回池
				m_AssetLoaderList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
			};
		}
		#endregion
	}
}
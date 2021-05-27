using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// 资源加载器
	/// </summary>
	public class AssetLoaderRoutine
	{
		/// <summary>
		/// 资源加载请求
		/// </summary>
		private AssetBundleRequest m_CurrAssetBundleRequest;

		private string m_CurrAssetName;

		/// <summary>
		/// 资源请求更新
		/// </summary>
		public Action<float> OnAssetUpdate;

		/// <summary>
		/// 加载资源完毕
		/// </summary>
		public Action<UnityEngine.Object> OnLoadAssetComplete;


		/// <summary>
		/// 异步加载资源
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetBundle"></param>
		internal void LoadAsset(string assetName, AssetBundle assetBundle)
		{
			if (assetName.LastIndexOf(".unity") != -1)
			{
				if (OnLoadAssetComplete != null) OnLoadAssetComplete(null);
				return;
			}
			m_CurrAssetName = assetName;
			m_CurrAssetBundleRequest = assetBundle.LoadAssetAsync(assetName);
		}

		/// <summary>
		/// 重置
		/// </summary>
		public void Reset()
		{
			m_CurrAssetBundleRequest = null;
		}

		/// <summary>
		/// 更新
		/// </summary>
		internal void OnUpdate()
		{
			UpdateAssetBundleRequest();
		}

		/// <summary>
		/// 更新 资源加载 请求
		/// </summary>
		private void UpdateAssetBundleRequest()
		{
			if (m_CurrAssetBundleRequest != null)
			{
				if (m_CurrAssetBundleRequest.isDone)
				{
					UnityEngine.Object obj = m_CurrAssetBundleRequest.asset;
					if (obj != null)
					{
						//GameEntry.Log(LogCategory.Resource, "资源=>{0} 加载完毕", m_CurrAssetName);
						Reset();//一定要早点Reset

						if (OnLoadAssetComplete != null) OnLoadAssetComplete(obj);
					}
					else
					{
						GameEntry.LogError("资源=>{0} 加载失败", m_CurrAssetName);
						Reset();//一定要早点Reset

						if (OnLoadAssetComplete != null) OnLoadAssetComplete(null);
					}
				}
				else
				{
					//加载进度
					if (OnAssetUpdate != null) OnAssetUpdate(m_CurrAssetBundleRequest.progress);
				}
			}
		}
	}
}
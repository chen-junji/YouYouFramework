using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
	/// <summary>
	/// AssetInfo的主Asset实体
	/// </summary>
	public class AssetInfoEntity
	{
		/// <summary>
		/// 资源完整名称(路径)
		/// </summary>
		public string AssetFullPath;

		/// <summary>
		/// 所属资源包(这个资源在哪一个Assetbundle里)
		/// </summary>
		public string AssetBundleFullPath;

		/// <summary>
		/// 依赖资源包列表
		/// </summary>
		public List<string> DependsAssetBundleList;
	}
}

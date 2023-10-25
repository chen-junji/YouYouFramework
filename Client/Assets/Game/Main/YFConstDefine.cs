using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
	/// <summary>
	/// 常量类
	/// </summary>
	public class YFConstDefine
	{
		/// <summary>
		/// 版本文件名称
		/// </summary>
		public const string VersionFileName = "VersionFile.bytes";

		/// <summary>
		/// 资源版本号
		/// </summary>
		public const string AssetVersion = "AssetVersion";

		/// <summary>
		/// 资源信息文件名称
		/// </summary>
		public const string AssetInfoName = "AssetInfo.bytes";

		/// <summary>
		/// 数据表的AssetBundle的存储路径
		/// </summary>
		public const string DataTableAssetBundlePath = "game/download/datatable.assetbundle";

		/// <summary>
		/// 热更程序集的Assetbundle的存储路径
		/// </summary>
		public const string HotfixAssetBundlePath = "game/download/hotfix.assetbundle";

		/// <summary>
		/// GameEntry的Assetbundle的存储路径
		/// </summary>
		public const string GameEntryAssetBundlePath = "game/download/hotfix.assetbundle";

		/// <summary>
		/// 自定义Shader的AssetBundle的存储路径
		/// </summary>
		public const string CusShadersAssetBundlePath = "game/download/shader.assetbundle";

		/// <summary>
		/// Http请求失败后的重试次数
		/// </summary>
		public const string Http_Retry = "Http_Retry";
		/// <summary>
		/// Http重试间隔
		/// </summary>
		public const string Http_RetryInterval = "Http_RetryInterval";

		/// <summary>
		/// Download请求失败后的重试次数
		/// </summary>
		public const string Download_Retry = "Download_Retry";
		/// <summary>
		/// Download重试间隔
		/// </summary>
		public const string Download_RetryInterval = "Download_RetryInterval";
		/// <summary>
		/// 下载多文件器的数量
		/// </summary>
		public const string Download_RoutineCount = "Download_RoutineCount";
		/// <summary>
		/// 断点续传的存储间隔缓存
		/// </summary>
		public const string Download_FlushSize = "Download_FlushSize";

		/// <summary>
		/// 类对象池_释放间隔
		/// </summary>
		public const string Pool_ReleaseClassObjectInterval = "Pool_ReleaseClassObjectInterval";
		/// <summary>
		/// AssetBundle池_释放间隔
		/// </summary>
		public const string Pool_ReleaseAssetBundleInterval = "Pool_ReleaseAssetBundleInterval";
		/// <summary>
		/// Asset池_释放间隔
		/// </summary>
		public const string Pool_ReleaseAssetInterval = "Pool_ReleaseAssetInterval";

		/// <summary>
		/// UI对象池中最大数量
		/// </summary>
		public const string UI_PoolMaxCount = "UI_PoolMaxCount";
		/// <summary>
		/// UI回池后过期时间_秒
		/// </summary>
		public const string UI_Expire = "UI_Expire";
		/// <summary>
		/// UI释放间隔_秒
		/// </summary>
		public const string UI_ClearInterval = "UI_ClearInterval";

	}
}
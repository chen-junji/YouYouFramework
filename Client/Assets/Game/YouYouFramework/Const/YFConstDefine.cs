using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
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
		public const string ResourceVersion = "ResourceVersion";

		/// <summary>
		/// 资源信息文件名称
		/// </summary>
		public const string AssetInfoName = "AssetInfo.bytes";

		/// <summary>
		/// 数据表的AssetBundle的存储路径
		/// </summary>
		public const string DataTableAssetBundlePath = "download/datatable.assetbundle";
		/// <summary>
		/// XLua的AssetBundle的存储路径
		/// </summary>
		public const string XLuaAssetBundlePath = "download/xlualogic.assetbundle";

		public const string ILRuntimeAssetBundlePath = "download/hotfix.assetbundle";
		/// <summary>
		/// 自定义Shader的AssetBundle的存储路径
		/// </summary>
		public const string CusShadersAssetBundlePath = "download/cusshaders.assetbundle";	
		/// <summary>
		/// 音频Bank文件的AssetBundle的存储路径
		/// </summary>
		public const string AudioAssetBundlePath = "download/fmod.assetbundle";

		/// <summary>
		/// 帧率
		/// </summary>
		public const string targetFrameRate = "targetFrameRate";

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

		/// <summary>
		/// Lua中可释放表数据的生命周期
		/// </summary>
		public const string Lua_DataTableLife = "Lua_DataTableLife";

		/// <summary>
		/// Audio释放间隔
		/// </summary>
		public const string Audio_ReleaseInterval = "Audio_ReleaseInterval";

	}
}
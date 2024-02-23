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
    }
}
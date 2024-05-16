using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYouMain
{
    /// <summary>
    /// 常量类
    /// </summary>
    public class MainConstDefine
    {
        /// <summary>
        /// 资源版本号
        /// </summary>
        public const string AssetVersion = "AssetVersion";

        /// <summary>
        /// 版本文件名称
        /// </summary>
        public const string VersionFileName = "VersionFile.bytes";

        /// <summary>
        /// 热更程序集的Assetbundle的存储路径
        /// </summary>
        public const string HotfixAssetBundlePath = "game/download/hotfix.assetbundle";


        /// <summary>
        /// 只读区加载AssetBundle的统一文件路径
        /// </summary>
        public static string StreamingAssetBundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");

        /// <summary>
        /// 可写区加载AssetBundle的统一文件路径
        /// </summary>
        public static string LocalAssetBundlePath = Application.persistentDataPath;

        /// <summary>
        /// 编辑器加载AssetBundle的统一文件路径
        /// </summary>
        public static string EditorAssetBundlePath = Application.dataPath;


        /// <summary>
        /// 可写区版本文件路径
        /// </summary>
        public static string LocalVersionFilePath = Path.Combine(LocalAssetBundlePath, VersionFileName);
    }
}
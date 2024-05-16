using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYouMain;

namespace YouYouFramework
{
    public class YFConstDefine
    {
        /// <summary>
        /// 资源依赖信息文件名称
        /// </summary>
        public const string AssetInfoName = "AssetInfo.bytes";

        /// <summary>
        /// 数据表的AssetBundle的存储路径
        /// </summary>
        public const string DataTableAssetBundlePath = "game/download/datatable.assetbundle";

        /// <summary>
        /// 自定义Shader的AssetBundle的存储路径
        /// </summary>
        public const string CusShadersAssetBundlePath = "game/download/shader.assetbundle";

        /// <summary>
        /// 可写区资源依赖信息文件路径
        /// </summary>
        public static string LocalAssetInfoPath = Path.Combine(MainConstDefine.LocalAssetBundlePath, AssetInfoName);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouMain
{
    /// <summary>
    /// AssetBundle版本文件, VersionFile.json的数据实体
    /// </summary>
    public class VersionFileEntity
    {
        /// <summary>
        /// 资源包的全路径
        /// </summary>
        public string AssetBundleFullPath;

        /// <summary>
        /// MD5码
        /// </summary>
        public string MD5;

        /// <summary>
        /// 文件大小(字节)
        /// </summary>
        public ulong Size;

        /// <summary>
        /// 是否初始数据
        /// </summary>
        public bool IsFirstData;

        /// <summary>
        /// 是否已经加密
        /// </summary>
        public bool IsEncrypt;

    }
}
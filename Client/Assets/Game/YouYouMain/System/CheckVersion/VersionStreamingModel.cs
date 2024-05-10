using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;
using YouYouMain;

/// <summary>
/// AssetBundle版本文件, StreamingAssets文件夹的VersionFile.bytes的Model
/// </summary>
public class VersionStreamingModel
{
    public static VersionStreamingModel Instance { get; private set; } = new VersionStreamingModel();

    /// <summary>
    /// 只读区资源版本号
    /// </summary>
    public string AssetsVersion;

    /// <summary>
    /// 只读区资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();

    public VersionFileEntity GetVersionFileEntity(string assetbundlePath)
    {
        VersionDic.TryGetValue(assetbundlePath, out VersionFileEntity entity);
        return entity;
    }

}

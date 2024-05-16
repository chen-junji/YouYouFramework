using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYouMain;


/// <summary>
/// AssetBundle版本文件, 本地可写区的VersionFile.json的Model
/// </summary>
public class VersionLocalModel
{
    public static VersionLocalModel Instance { get; private set; } = new VersionLocalModel();

    /// <summary>
    /// 可写区资源版本号
    /// </summary>
    public string AssetVersion;

    /// <summary>
    /// 可写区资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();

    public VersionFileEntity GetVersionFileEntity(string assetbundlePath)
    {
        VersionDic.TryGetValue(assetbundlePath, out VersionFileEntity entity);
        return entity;
    }

    /// <summary>
    /// 保存可写区版本信息
    /// </summary>
    public void SaveVersion()
    {
        //保存版本文件
        string json = VersionDic.ToJson();
        IOUtil.CreateTextFile(MainConstDefine.LocalVersionFilePath, json);
    }

    /// <summary>
    /// 保存可写区资源版本号
    /// </summary>
    public void SetAssetVersion(string version)
    {
        AssetVersion = version;
        PlayerPrefs.SetString(MainConstDefine.AssetVersion, version);
    }

}

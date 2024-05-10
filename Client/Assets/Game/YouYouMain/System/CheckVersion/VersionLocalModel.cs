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
    public string AssetsVersion;

    /// <summary>
    /// 可写区资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();

    /// <summary>
    /// 可写区 版本文件路径
    /// </summary>
    public string VersionFilePath
    {
        get
        {
            return Path.Combine(YFConstDefine.LocalAssetBundlePath, YFConstDefine.VersionFileName);
        }
    }

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
        IOUtil.CreateTextFile(VersionFilePath, json);
    }

    /// <summary>
    /// 保存可写区资源版本号
    /// </summary>
    public void SetAssetVersion(string version)
    {
        AssetsVersion = version;
        PlayerPrefs.SetString(YFConstDefine.AssetVersion, version);
    }

}

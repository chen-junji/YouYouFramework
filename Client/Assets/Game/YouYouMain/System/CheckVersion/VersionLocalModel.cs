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
    /// 可写区 版本文件路径
    /// </summary>
    public string VersionFilePath
    {
        get
        {
            return string.Format("{0}/{1}", Application.persistentDataPath, YFConstDefine.VersionFileName);
        }
    }

    /// <summary>
    /// 可写区资源版本号
    /// </summary>
    public string AssetsVersion;

    /// <summary>
    /// 可写区资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();

    /// <summary>
    /// 保存可写区版本信息
    /// </summary>
    public void SaveVersion(VersionFileEntity entity)
    {
        VersionDic[entity.AssetBundleName] = entity;

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

    /// <summary>
    /// 获取可写区版本文件是否存在
    /// </summary>
    public bool GetVersionFileExists()
    {
        return File.Exists(VersionFilePath);
    }
}

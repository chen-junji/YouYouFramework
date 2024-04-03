using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouMain;


/// <summary>
/// AssetBundle版本文件, 云端资源站点的VersionFile.bytes的Model
/// </summary>
public class VersionCDNModel
{
    public static VersionCDNModel Instance { get; private set; } = new VersionCDNModel();

    /// <summary>
    /// CDN资源版本号
    /// </summary>
    public string CDNVersion;

    /// <summary>
    /// CDN资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();


    /// <summary>
    /// 获取CDN上的资源包的版本信息(这个方法一定要能返回信息)
    /// </summary>
    /// <param name="assetbundlePath"></param>
    public VersionFileEntity GetVersionFileEntity(string assetbundlePath)
    {
        VersionFileEntity entity = null;
        VersionDic.TryGetValue(assetbundlePath, out entity);
        return entity;
    }
}

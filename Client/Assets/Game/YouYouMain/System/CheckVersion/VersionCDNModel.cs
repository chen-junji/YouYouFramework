using System;
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
    public string AssetVersion;

    /// <summary>
    /// CDN资源包信息
    /// </summary>
    public Dictionary<string, VersionFileEntity> VersionDic = new Dictionary<string, VersionFileEntity>();


    /// <summary>
    /// 获取CDN上的资源包的版本信息(这个方法一定要能返回信息)
    /// </summary>
    public VersionFileEntity GetVersionFileEntity(string assetbundlePath)
    {
        VersionDic.TryGetValue(assetbundlePath, out VersionFileEntity entity);
        return entity;
    }

    /// <summary>
    /// 单个文件检查更新(True==不需要更新)
    /// </summary>
    public bool CheckVersionChangeSingle(string assetBundleName)
    {
        if (VersionDic.TryGetValue(assetBundleName, out VersionFileEntity cdnAssetBundleInfo))
        {
            if (VersionLocalModel.Instance.VersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleFullPath, out VersionFileEntity LocalAssetsAssetBundleInfo))
            {
                //可写区有 CDN也有 验证MD5
                return cdnAssetBundleInfo.MD5.Equals(LocalAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase);
            }
        }
        return false;//CDN不存在
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// Asset实体
/// </summary>
public class AssetEntity
{
    /// <summary>
    /// 资源分类
    /// </summary>
    public AssetCategory Category;

    /// <summary>
    /// 资源名称
    /// </summary>
    public string AssetName;

    /// <summary>
    /// 资源完整名称(路径)
    /// </summary>
    public string AssetFullName;

    /// <summary>
    /// 所属资源包(这个资源在哪一个Assetbundle里)
    /// </summary>
    public string AssetBundleName;
    
    /// <summary>
    /// 依赖资源
    /// </summary>
    public List<AssetDependsEntity> DependsAssetList;
}


using System.Collections;

/// <summary>
/// Sys_Prefab实体
/// </summary>
public partial class Sys_PrefabEntity : DataTableEntityBase
{
    /// <summary>
    /// Name
    /// </summary>
    public string Name;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// 后缀
    /// </summary>
    public string Suffix;

    /// <summary>
    /// 对象池编号
    /// </summary>
    public byte PoolId;

    /// <summary>
    /// 是否开启缓存池自动清理模式
    /// </summary>
    public byte CullDespawned;

    /// <summary>
    /// 缓存池自动清理但是始终保留几个对象不清理
    /// </summary>
    public int CullAbove;

    /// <summary>
    /// 多长时间清理一次单位是秒
    /// </summary>
    public int CullDelay;

    /// <summary>
    /// 每次清理几个
    /// </summary>
    public int CullMaxPerPass;

}

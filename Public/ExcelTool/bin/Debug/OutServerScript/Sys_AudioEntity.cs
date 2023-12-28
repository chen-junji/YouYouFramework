
using System.Collections;

/// <summary>
/// Sys_Audio实体
/// </summary>
public partial class Sys_AudioEntity : DataTableEntityBase
{
    /// <summary>
    /// 路径
    /// </summary>
    public string AssetFullPath;

    /// <summary>
    /// 音量（0-1）
    /// </summary>
    public float Volume;

    /// <summary>
    /// 优先级(默认128)
    /// </summary>
    public byte Priority;

}

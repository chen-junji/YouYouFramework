
//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：2020-09-05 03:03:09
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;

/// <summary>
/// Sys_Effect实体
/// </summary>
public partial class Sys_EffectEntity : DataTableEntityBase
{
    /// <summary>
    /// 描述
    /// </summary>
    public string Desc;

    /// <summary>
    /// 预设编号
    /// </summary>
    public int PrefabId;

    /// <summary>
    /// 持续时间(秒)
    /// </summary>
    public float KeepTime;

    /// <summary>
    /// 声音编号
    /// </summary>
    public int SoundId;

    /// <summary>
    /// 特效类型(0=普通1=抛物线)
    /// </summary>
    public int Type;

}

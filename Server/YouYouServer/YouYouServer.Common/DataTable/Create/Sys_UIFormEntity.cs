
//===================================================
//作    者：边涯  http://www.u3dol.com
//备    注：此代码为工具生成 请勿手工修改
//===================================================
using System.Collections;

/// <summary>
/// Sys_UIForm实体
/// </summary>
public partial class Sys_UIFormEntity : DataTableEntityBase
{
    /// <summary>
    /// 描述
    /// </summary>
    public string Desc;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name;

    /// <summary>
    /// UI分组编号
    /// </summary>
    public byte UIGroupId;

    /// <summary>
    /// 禁用层级管理
    /// </summary>
    public int DisableUILayer;

    /// <summary>
    /// 是否锁定
    /// </summary>
    public int IsLock;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath_Chinese;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath_English;

    /// <summary>
    /// 允许多实例
    /// </summary>
    public int CanMulit;

    /// <summary>
    /// 显示类型0=普通1=反切
    /// </summary>
    public byte ShowMode;

    /// <summary>
    /// 冻结类型0=置空层1=禁用
    /// </summary>
    public byte FreezeMode;

    /// <summary>
    /// 打开动画0=无1=缩放
    /// </summary>
    public byte OpenAnim;

}

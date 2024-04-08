
/// <summary>
/// 动态目标点
/// </summary>
public enum DynamicTarget
{
    /// <summary>
    /// 我方单体
    /// </summary>
    OurOne,
    /// <summary>
    /// 我方全体
    /// </summary>
    OurAll,
    /// <summary>
    /// 我方队友
    /// </summary>
    OuTeammate,

    /// <summary>
    /// 敌人单体
    /// </summary>
    EnemyOne,
    /// <summary>
    /// 敌人全体
    /// </summary>
    EnemyAll,
    /// <summary>
    /// 敌人队友
    /// </summary>
    EnemyTeammate
}

[System.Serializable]
public enum BuffCategory
{
    None = 0,
    /// <summary>
    /// 眩晕
    /// </summary>
    XuanYun = 1,
}
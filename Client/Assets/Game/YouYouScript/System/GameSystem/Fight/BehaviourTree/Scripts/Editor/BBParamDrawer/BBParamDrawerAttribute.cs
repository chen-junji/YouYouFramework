using System;

/// <summary>
/// 黑板参数Drawer特性
/// </summary>
public class BBParamDrawerAttribute : Attribute
{
    /// <summary>
    /// 黑板参数类型
    /// </summary>
    public Type BBParamType;
}
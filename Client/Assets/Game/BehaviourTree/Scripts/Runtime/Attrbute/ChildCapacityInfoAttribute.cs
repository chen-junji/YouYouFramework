using System;

/// <summary>
/// 子节点容量
/// </summary>
public enum ChildCapacity
{
    /// <summary>
    /// 无
    /// </summary>
    None,
        
    /// <summary>
    /// 一个
    /// </summary>
    Single,
        
    /// <summary>
    /// 多个
    /// </summary>
    Multi,
}

/// <summary>
/// 子节点容量信息特性
/// </summary>
public class ChildCapacityInfoAttribute : Attribute
{
    public ChildCapacity Capacity;
}
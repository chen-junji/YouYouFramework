using System;
using System.Collections.Generic;

/// <summary>
/// 动作节点基类
/// </summary>
[ChildCapacityInfo(Capacity = ChildCapacity.None)]
public abstract class BaseActionNode : BaseNode
{
    /// <inheritdoc />
    public sealed override void AddChild(BaseNode child)
    {

    }

    /// <inheritdoc />
    public sealed override void RemoveChild(BaseNode child)
    {

    }

    /// <inheritdoc />
    public sealed override void ClearChild()
    {

    }

    /// <inheritdoc />
    public sealed override void ForeachChild(Action<BaseNode> action)
    {

    }

    /// <inheritdoc />
    public sealed override void ClearId()
    {
        base.ClearId();
    }

    /// <inheritdoc />
    public sealed override void RebuildId()
    {

    }

    /// <inheritdoc />
    public sealed override void RebuildNodeReference(List<BaseNode> allNodes)
    {

    }

    /// <inheritdoc />
    protected sealed override void OnChildFinished(BaseNode child, bool success)
    {

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装饰节点基类
/// </summary>
[ChildCapacityInfo(Capacity = ChildCapacity.Single)]
public abstract class BaseDecoratorNode : BaseNode
{
    /// <summary>
    /// 子节点ID
    /// </summary>
    public int ChildId;

    /// <summary>
    /// 子节点
    /// </summary>
    [NonSerialized] 
    public BaseNode Child;

    /// <inheritdoc />
    public override void AddChild(BaseNode child)
    {
        if (child == null)
        {
            return;
        }

        if (child.ParentNode != null)
        {
            child.ParentNode.RemoveChild(child);
        }

        child.ParentNode = this;
        Child = child;
    }

    /// <inheritdoc />
    public override void RemoveChild(BaseNode child)
    {
        if (child == null)
        {
            return;
        }
        
        child.ParentNode = null;
        Child = null;
    }

    /// <inheritdoc />
    public override void ClearChild()
    {
        RemoveChild(Child);
    }

    /// <inheritdoc />
    public override void ForeachChild(Action<BaseNode> action)
    {
        action?.Invoke(Child);
    }

    /// <inheritdoc />
    public override void ClearId()
    {
        base.ClearId();
        ChildId = 0;
    }

    /// <inheritdoc />
    public override void RebuildId()
    {
        if (Child != null)
        {
            ChildId = Child.Id;
        }
        else
        {
            ChildId = 0;
        }
    }

    /// <inheritdoc />
    public override void RebuildNodeReference(List<BaseNode> allNodes)
    {
        if (ChildId == 0)
        {
            return;
        }

        BaseNode child = allNodes[ChildId - 1];
        AddChild(child);
    }

    /// <inheritdoc />
    protected override void OnStart()
    {
        Child.Start();
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Child.Cancel();
    }

    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        Finish(success);
    }
}

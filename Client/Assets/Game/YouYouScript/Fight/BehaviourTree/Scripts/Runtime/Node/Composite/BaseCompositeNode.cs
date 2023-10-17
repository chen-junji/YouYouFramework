using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 复合节点基类
/// </summary>
[ChildCapacityInfo(Capacity = ChildCapacity.Multi)]
public abstract class BaseCompositeNode : BaseNode
{
    /// <summary>
    /// 子节点ID列表
    /// </summary>
    public List<int> ChildIdList = new List<int>();

    /// <summary>
    /// 子节点列表
    /// </summary>
    [NonSerialized] 
    public List<BaseNode> Children = new List<BaseNode>();

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
        Children.Add(child);
    }

    /// <inheritdoc />
    public override void RemoveChild(BaseNode child)
    {
        if (child == null)
        {
            return;
        }

        child.ParentNode = null;
        Children.Remove(child);
    }

    /// <inheritdoc />
    public override void ClearChild()
    {
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            RemoveChild(Children[i]);
        }
    }

    /// <inheritdoc />
    public override void ForeachChild(Action<BaseNode> action)
    {
        foreach (var child in Children)
        {
            action?.Invoke(child);
        }
    }

    /// <inheritdoc />
    public override void SortChild()
    {
        Children.Sort((a, b) =>
        {
            if (Math.Abs(a.Position.x - b.Position.x) > float.Epsilon)
            {
                return a.Position.x.CompareTo(b.Position.x);
            }

            return a.Id.CompareTo(b.Id);
        });
    }

    /// <inheritdoc />
    public override void ClearId()
    {
        base.ClearId();
        ChildIdList.Clear();
    }

    /// <inheritdoc />
    public override void RebuildId()
    {
        ChildIdList.Clear();
        foreach (var child in Children)
        {
            ChildIdList.Add(child.Id);
        }
    }

    /// <inheritdoc />
    public override void RebuildNodeReference(List<BaseNode> allNodes)
    {
        Children.Clear();
        foreach (int childId in ChildIdList)
        {
            if (childId == 0)
            {
                continue;
            }
            BaseNode child = allNodes[childId - 1];
            AddChild(child);
        }
    }
}

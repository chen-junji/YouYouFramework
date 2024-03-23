using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注释块
/// </summary>
[Serializable]
public class CommentBlock
{
    /// <summary>
    /// 注释
    /// </summary>
    public string Comment;
        
    /// <summary>
    /// 颜色
    /// </summary>
    public Color Color = new Color(0, 0, 0, 0.3f);
        
    /// <summary>
    /// 位置
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 注释块内的节点列表
    /// </summary>
    [NonSerialized]
    public List<BaseNode> Nodes;

    /// <summary>
    /// 注释块内的节点Id列表
    /// </summary>
    public List<int> NodeIds;

    public CommentBlock()
    {
        Nodes = new List<BaseNode>();
        NodeIds = new List<int>();
    }
        
    public CommentBlock(string comment, Vector2 position) : this()
    {
        Comment = comment;
        Position = position;
    }

    /// <summary>
    /// 重建注释块包含的节点id
    /// </summary>
    public void RebuildId()
    {
        if (Nodes.Count == 0)
        {
            return;
        }
            
        NodeIds.Clear();
        foreach (BaseNode node in Nodes)
        {
            NodeIds.Add(node.Id);
        }
    }

    /// <summary>
    /// 重建对注释块包含的节点的引用
    /// </summary>
    public void RebuildNodeReference(List<BaseNode> allNodes)
    {
        Nodes.Clear();
        foreach (int id in NodeIds)
        {
            BaseNode node = allNodes[id - 1];
            Nodes.Add(node);
        }
    }
}
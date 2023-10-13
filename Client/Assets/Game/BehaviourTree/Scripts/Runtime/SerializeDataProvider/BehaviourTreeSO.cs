using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于Unity ScriptableObject的行为树序列化数据提供者
/// </summary>
public class BehaviourTreeSO : ScriptableObject, IBehaviourTreeSerializeDataProvider
{

    // <summary>
    /// 行为树视口的位置与缩放
    /// </summary>
    public Rect ViewportRect;

    /// <summary>
    /// 节点属性面板宽度
    /// </summary>
    public float InspectorWidth;

    /// <summary>
    /// 黑板位置与大小
    /// </summary>
    public Rect BlackBoardRect;

    /// <summary>
    /// 根节点ID
    /// </summary>
    public int RootNodeId;

    /// <summary>
    /// 节点列表
    /// </summary>
    [SerializeReference] 
    public List<BaseNode> AllNodes = new List<BaseNode>();

    /// <summary>
    /// 黑板参数列表
    /// </summary>
    [SerializeReference] 
    public List<BBParam> BBParams = new List<BBParam>();
    
    /// <summary>
    /// 注释块列表
    /// </summary>
    public List<CommentBlock> CommentBlocks = new List<CommentBlock>();

    public void Serialize(BehaviourTree behaviourTree)
    {
        behaviourTree.SerializePreProcess();
        RootNodeId = behaviourTree.RootNodeId;
        AllNodes = behaviourTree.AllNodes;

        //黑板参数
        BBParams.Clear();
        foreach (var pair in behaviourTree.BlackBoard.ParamDict)
        {
            BBParams.Add(pair.Value);
        }
    }

    public BehaviourTree Deserialize()
    {
        BehaviourTree behaviourTree = new BehaviourTree();
        behaviourTree.RootNodeId = RootNodeId;
        behaviourTree.AllNodes = AllNodes;
        behaviourTree.DeserializePostProcess();

        //黑板参数
        behaviourTree.BlackBoard = new BlackBoard();
        foreach (BBParam param in BBParams)
        {
            behaviourTree.BlackBoard.SetParam(param.Key, param);
        }
        
        //重建对注释块包含的节点的引用
        foreach (var commentBlock in CommentBlocks)
        {
            commentBlock.RebuildNodeReference(AllNodes);
        }

        return behaviourTree;
    }

    /// <summary>
    /// 复制行为树
    /// </summary>
    public BehaviourTree CloneBehaviourTree()
    {
        var instance = Instantiate(this);
        var bt = instance.Deserialize();
        return bt;
    }

#if UNITY_EDITOR

    /// <summary>
    /// 创建行为树节点
    /// </summary>
    public BaseNode CreateNode(Type nodeType)
    {
        BaseNode node = (BaseNode)Activator.CreateInstance(nodeType);
        AllNodes.Add(node);
        return node;
    }

    /// <summary>
    /// 删除行为树节点
    /// </summary>
    public void RemoveNode(BaseNode node)
    {
        AllNodes.Remove(node);
    }

    /// <summary>
    /// 构建节点Id
    /// </summary>
    public void BuildNodeId()
    {
        //为所有节点建立Id 并排序子节点
        for (int i = 0; i < AllNodes.Count; i++)
        {
            int id = i + 1;
            BaseNode node = AllNodes[i];
            node.Id = id;
            node.SortChild();
        }

        foreach (BaseNode node in AllNodes)
        {
            node.RebuildId();
        }
        
        foreach (var commentBlock in CommentBlocks)
        {
            commentBlock.RebuildId();
        }
    }

    /// <summary>
    /// 获取所有黑板参数key
    /// </summary>
    public List<string> GetAllParamKey()
    {
        List<string> keyList = new List<string>();
        foreach (var param in BBParams)
        {
            keyList.Add(param.Key);
        }

        return keyList;
    }

    /// <summary>
    /// 根据类型获取黑板参数key数组
    /// </summary>
    public string[] GetParamKeys(Type type)
    {
        List<string> keyList = new List<string>();

        for (int i = 0; i < BBParams.Count; i++)
        {
            var param = BBParams[i];
            if (param.GetType() == type)
            {
                keyList.Add(param.Key);
            }
        }

        keyList.Sort();
        keyList.Insert(0, "Null");

        return keyList.ToArray();
    }

    /// <summary>
    /// 是否包含黑板参数Key
    /// </summary>
    public bool ContainsParamKey(string key)
    {
        for (int i = 0; i < BBParams.Count; i++)
        {
            if (BBParams[i].Key == key)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 设置黑板参数
    /// </summary>
    public void SetParam(string key, BBParam param)
    {
        if (param == null)
        {
            return;
        }

        param.Key = key;

        for (int i = 0; i < BBParams.Count; i++)
        {
            if (BBParams[i].Key == key)
            {
                BBParams[i] = param;
                return;
            }
        }

        BBParams.Add(param);
    }

    /// <summary>
    /// 移除黑板参数
    /// </summary>
    public void RemoveParam(string key)
    {
        for (int i = BBParams.Count - 1; i >= 0; i--)
        {
            if (BBParams[i].Key == key)
            {
                BBParams.RemoveAt(i);
                return;
            }
        }
    }
#endif

}
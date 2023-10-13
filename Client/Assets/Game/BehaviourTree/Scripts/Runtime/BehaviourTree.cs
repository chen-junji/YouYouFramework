using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 行为树
/// </summary>
public class BehaviourTree
{
    static BehaviourTree()
    {
        GameObject go = new GameObject();
        go.AddComponent<BehaviourTreeComponent>();
        go.hideFlags = HideFlags.HideInHierarchy;
    }
    
    /// <summary>
    /// 行为树运行结束回调
    /// </summary>
    public event Action<bool> OnFinished
    {
        add
        {
            if (RootNode == null)
            {
                return;
            }

            RootNode.OnFinished += value;
        }

        remove
        {
            if (RootNode == null)
            {
                return;
            }

            RootNode.OnFinished -= value;
        }
    }

    /// <summary>
    /// 行为树被取消回调
    /// </summary>
    public Action OnCanceled;

    /// <summary>
    /// 行为树调试名
    /// </summary>
    public string DebugName;
    
    /// <summary>
    /// 根节点ID
    /// </summary>
    public int RootNodeId;
    
    /// <summary>
    /// 根节点
    /// </summary>
    public RootNode RootNode;
    
    /// <summary>
    /// 节点列表
    /// </summary>
    public List<BaseNode> AllNodes = new List<BaseNode>();
    
    /// <summary>
    /// 黑板
    /// </summary>
    public BlackBoard BlackBoard = new BlackBoard();
    
    /// <summary>
    /// 是否已初始化
    /// </summary>
    private bool isInit;

    /// <summary>
    /// 初始化行为树
    /// </summary>
    private void Init()
    {
        //重建节点对黑板参数的引用
        foreach (BaseNode node in AllNodes)
        {
            node.RebuildBBParamReference();
        }
    }
    
    /// <summary>
    /// 开始运行行为树
    /// </summary>
    public void Start(string debugName = null)
    {
        if (RootNode == null)
        {
            throw new Exception("行为树运行失败，根节点为空");
        }

        if (RootNode.CurState == BaseNode.State.Running)
        {
            return;
        }
        
        if (!isInit)
        {
            isInit = true;
            Init();
        }
        else
        {
            //第二次被启动 重置所有节点运行状态为Free
            ResetAllNode();
        }
        
#if UNITY_EDITOR
        DebugName = debugName;
        if (string.IsNullOrEmpty(DebugName))
        {
            DebugName = DateTime.Now.ToString();
        }
        DebugName = DebugName.Replace('/', '\\');
#endif
        
        foreach (BaseNode node in AllNodes)
        {
            node.OnBehaviourTreeStart();
        }

        RootNode.Start();
    }

    /// <summary>
    /// 取消运行行为树
    /// </summary>
    public void Cancel()
    {
        if (RootNode.CurState != BaseNode.State.Running)
        {
            return;
        }

        RootNode.Cancel();
        OnCanceled?.Invoke();
    }
    
    /// <summary>
    /// 重启行为树
    /// </summary>
    public void Restart()
    {
        Cancel();
        RootNode.Start();
    }
    
    private void ResetAllNode()
    {
        foreach (var node in AllNodes)
        {
            node.CurState = BaseNode.State.Free;
        }
    }
    
    /// <summary>
    /// 获取行为树节点
    /// </summary>
    private BaseNode GetNode(int nodeId)
    {
        if (nodeId == 0)
        {
            return null;
        }
            
        return AllNodes[nodeId - 1];
    }


    /// <summary>
    /// 序列化前的预处理
    /// </summary>
    public void SerializePreProcess()
    {
        //为所有节点建立Id 并排序子节点
        for (int i = 0; i < AllNodes.Count; i++)
        {
            int id = i + 1;
            BaseNode node = AllNodes[i];
            node.Id = id;   
            node.SortChild();
        }
            
        //重建父子Id
        if (RootNode != null)
        {
            RootNodeId = RootNode.Id;
        }
        foreach (BaseNode node in AllNodes)
        {
            node.RebuildId();
        }
    }

    /// <summary>
    /// 反序列化的后处理
    /// </summary>
    public void DeserializePostProcess()
    {
        RootNode = (RootNode)GetNode(RootNodeId);
            
        foreach (BaseNode node in AllNodes)
        {
            //重建对父子节点和Owner的引用
            node.Owner = this;
            node.RebuildNodeReference(AllNodes);  
        }
    }
}

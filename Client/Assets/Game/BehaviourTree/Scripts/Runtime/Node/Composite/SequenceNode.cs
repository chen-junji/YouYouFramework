using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 序列节点（依次执行子节点，直到有子节点运行失败，则此节点运行失败，否则成功）
/// </summary>
[NodeInfo(Name = "序列",Desc = "依次执行子节点，直到有子节点运行失败，则此节点运行失败，否则成功")]
public class SequenceNode : BaseCompositeNode
{
    /// <summary>
    /// 当前子节点索引
    /// </summary>
    private int curChildIndex = -1;
    
    /// <inheritdoc />
    protected override void OnStart()
    {
        curChildIndex = -1;
        StartNextChild();
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        BaseNode curChild = Children[curChildIndex];
        curChild.Cancel();
    }

    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        if (success)
        {
            //子节点运行成功 继续下一个
            StartNextChild();
        }
        else
        {
            //子节点运行失败 此节点也失败了
            Finish(false);
        }
    }
    
    /// <summary>
    /// 开始运行下一个子节点
    /// </summary>
    private void StartNextChild()
    {
        curChildIndex++;
            
        if (curChildIndex >= Children.Count)
        {
            //所有子节点都运行成功 此节点运行成功
            Finish(true);
            return;
        }

        BaseNode curChild = Children[curChildIndex];
        curChild.Start();
    }
}

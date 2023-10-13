
/// <summary>
/// 并行节点（同时运行所有节点，根据设置的并行条件决定此节点如何判断成功/失败）
/// </summary>
[NodeInfo(Name = "并行",Desc = "同时运行所有节点，根据设置的并行条件决定此节点如何判断成功/失败")]
public class ParallelNode : BaseCompositeNode
{
    /// <summary>
    /// 并行节点的成功/失败条件
    /// </summary>
    public enum ParallelCondition
    {
        /// <summary>
        /// 任意一个子节点成功，则此节点成功，否则当所有子节点失败时，此节点失败
        /// </summary>
        FirstSuccess,
            
        /// <summary>
        /// 任意一个子节点失败，则此节点失败，否则当所有子节点成功时，此节点成功
        /// </summary>
        FirstFailure,
    }

    /// <summary>
    /// 成功/失败条件
    /// </summary>
    public ParallelCondition Condition;
    
    /// <summary>
    /// 运行成功的子节点数
    /// </summary>
    private int successCount;
        
    /// <summary>
    /// 运行失败的子节点数
    /// </summary>
    private int failedCount;
    
    /// <inheritdoc />
    protected override void OnStart()
    {
        successCount = 0;
        failedCount = 0;
            
        foreach (BaseNode child in Children)
        {
            child.Start();
        }
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        foreach (BaseNode child in Children)
        {
            child.Cancel();
        }
    }
        
    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        if (success)
        {
            successCount++;
        }
        else
        {
            failedCount++;
        }

        switch (Condition)
        {
            case ParallelCondition.FirstSuccess:

                if (successCount > 0)
                {
                    //一个子节点成功 此节点成功
                    Cancel();
                    Finish(true);
                }
                else if (failedCount == Children.Count)
                {
                    //所有子节点失败 此节点失败
                    Cancel();
                    Finish(false);
                }

                break;

            case ParallelCondition.FirstFailure:
                if (failedCount > 0)
                {
                    //一个子节点失败 此节点失败
                    Cancel();
                    Finish(false);
                }
                else if (successCount == Children.Count)
                {
                    //所有子节点成功 此节点成功
                    Cancel();
                    Finish(true);
                }

                break;
        }
    }
}
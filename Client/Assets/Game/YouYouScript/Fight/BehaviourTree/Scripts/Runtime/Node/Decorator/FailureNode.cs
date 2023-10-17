/// <summary>
/// 失败节点（无论子节点运行结果是什么，此节点都运行失败）
/// </summary>
[NodeInfo(Name = "失败",Desc = "无论子节点运行结果是什么，此节点都运行失败")]
public class FailureNode : BaseDecoratorNode
{
    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        Finish(false);
    }
}
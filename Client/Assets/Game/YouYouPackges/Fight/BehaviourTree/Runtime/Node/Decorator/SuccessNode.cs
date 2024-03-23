
/// <summary>
/// 成功节点（无论子节点运行结果是什么，此节点都运行成功）
/// </summary>
[NodeInfo(Name = "成功",Desc = "无论子节点运行结果是什么，此节点都运行成功")]
public class SuccessNode : BaseDecoratorNode
{
    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        Finish(true);
    }
}
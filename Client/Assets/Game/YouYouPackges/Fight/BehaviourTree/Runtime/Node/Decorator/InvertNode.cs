
/// <summary>
/// 反转节点（对子节点运行结果取反）
/// </summary>
[NodeInfo(Name = "反转",Desc = "对子节点运行结果取反")]
public class InvertNode : BaseDecoratorNode
{
    /// <inheritdoc />
    protected override void OnChildFinished(BaseNode child, bool success)
    {
        Finish(!success);
    }
}
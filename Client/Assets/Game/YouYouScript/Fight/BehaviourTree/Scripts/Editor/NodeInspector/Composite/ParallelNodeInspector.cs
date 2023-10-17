using UnityEditor;

/// <summary>
/// 并行节点属性面板Inspector
/// </summary>
[NodeInspector(NodeType = typeof(ParallelNode))]
public class ParallelNodeInspector : BaseNodeInspector
{
    /// <inheritdoc />
    public override void OnGUI()
    {
        base.OnGUI();
        ParallelNode parallelNode = (ParallelNode)Target;
        parallelNode.Condition = (ParallelNode.ParallelCondition)EditorGUILayout.EnumPopup("并行条件", parallelNode.Condition);
        EditorGUILayout.LabelField("FirstSuccess：任意一个子节点成功，则此节点成功，否则当所有子节点失败时，此节点失败");
        EditorGUILayout.LabelField("FirstFailure：任意一个子节点失败，则此节点失败，否则当所有子节点成功时，此节点成功");
    }
}
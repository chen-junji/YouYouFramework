using UnityEditor;

/// <summary>
/// 延时节点属性面板Inspector
/// </summary>
[NodeInspector(NodeType = typeof(DelayNode))]
public class DelayNodeInspector : BaseNodeInspector
{
    /// <inheritdoc />
    public override void OnGUI()
    {
        base.OnGUI();

        DelayNode delayNode = (DelayNode)Target;
            
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"当前计时：{delayNode.Timer:f2}");
    }
}
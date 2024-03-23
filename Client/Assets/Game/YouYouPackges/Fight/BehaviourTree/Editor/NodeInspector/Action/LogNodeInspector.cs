using UnityEditor;

/// <summary>
/// 日志节点属性面板Inspector
/// </summary>
[NodeInspector(NodeType = typeof(LogNode))]
public class LogNodeInspector : BaseNodeInspector
{
    /// <inheritdoc />
    public override void OnGUI()
    {
        base.OnGUI();
            
        LogNode logNode = (LogNode)Target;
        logNode.Level = (LogNode.LogLevel)EditorGUILayout.EnumPopup("日志级别", logNode.Level);
        EditorGUILayout.Space();
    }
}
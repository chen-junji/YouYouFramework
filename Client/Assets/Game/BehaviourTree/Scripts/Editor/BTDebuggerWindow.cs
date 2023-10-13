using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 行为树调试器窗口
/// </summary>
public class BTDebuggerWindow : EditorWindow
{
    private int curIndex = 0;
        
    [MenuItem("BehaviourTree/打开调试器窗口")]
    public static void Open()
    {
        var window = GetWindow<BTDebuggerWindow>("行为树调试器");
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("游戏运行后才能调试",MessageType.Warning);
            return;
        }
            
        if (BTDebugger.BTInstanceDict.Count == 0)
        {
            EditorGUILayout.HelpBox("没有注册到调试器里的行为树实例",MessageType.Info);
            return;
        }

        curIndex = EditorGUILayout.Popup("行为树实例：", curIndex, BTDebugger.BTInstanceDict.Keys.ToArray());

        if (GUILayout.Button("查看行为树运行状态"))
        {
            string name = BTDebugger.BTInstanceDict.Keys.ToArray()[curIndex];
            var bt = BTDebugger.Get(name);
            BehaviourTreeWindow.OpenFromDebugger(bt);
        }
    }
}
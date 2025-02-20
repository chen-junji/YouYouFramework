using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MacroSettings))]
public class MacroSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 默认属性绘制（如 value 字段）
        base.OnInspectorGUI();

        // 添加按钮
        MacroSettings script = (MacroSettings)target;
        if (GUILayout.Button("保存宏设置"))
        {
            script.SaveMacro();
        }
    }
}

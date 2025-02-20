using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "YouYouAsset/MacroSettings")]
public class MacroSettings : ScriptableObject
{
    [Serializable]
    public class MacroData
    {
        [TableColumnWidth(80, Resizable = false)]
        public bool Enabled;

        /// <summary>
        /// 参数设置的Key(中文介绍)
        /// </summary>
        public string Name;

        /// <summary>
        /// 参数设置的值
        /// </summary>
        public string Macro;
    }
    [PropertySpace(10)]
    [BoxGroup("MacroSettings")]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [HideLabel]
    public MacroData[] Settings;

    private string m_Macor;

    [Button(ButtonSizes.Medium), ResponsiveButtonGroup("DefaultButtonSize"), PropertyOrder(1)]
    public void SaveMacro()
    {
#if UNITY_EDITOR
        string macor = string.Empty;
        foreach (var item in Settings)
        {
            if (item.Enabled)
            {
                macor += string.Format("{0};", item.Macro);
            }
        }


        List<string> definesL = new();
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] defines);
        for (int i = 0; i < defines.Length; i++) definesL.Add(defines[i]);
        foreach (var item in Settings) definesL.Remove(item.Macro);
        for (int i = 0; i < definesL.Count; i++) macor += string.Format("{0};", definesL[i]);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macor);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, macor);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, macor);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, macor);
        AssetDatabase.SaveAssets();
        Debug.Log("Sava Macro Success====" + macor);
#endif
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        //初始化m_Macor
        m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (!string.IsNullOrEmpty(m_Macor))
        {
            //该字符串包含Settings[i].Macro
            for (int i = 0; i < Settings.Length; i++)
            {
                if (m_Macor.IndexOf(Settings[i].Macro) != -1)
                {
                    Settings[i].Enabled = true;
                }
                else
                {
                    Settings[i].Enabled = false;
                }
            }
        }
#endif
    }

}


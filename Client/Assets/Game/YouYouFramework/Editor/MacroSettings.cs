using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu]
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
    /// <summary>
    /// 资源加载方式
    /// </summary>
    public enum AssetLoadTarget
    {
        RESOURCES = 0,
        ASSETBUNDLE = 1,
        EDITORLOAD = 2,
    }
    private string m_Macor;

    [LabelText("资源加载方式")]
    public AssetLoadTarget CurrAssetLoadTarget;

    [PropertySpace(10)]
    [BoxGroup("MacroSettings")]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [HideLabel]
    public MacroData[] Settings;


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
        macor += string.Format("{0};", CurrAssetLoadTarget.ToString());

        //设置BuildSetting中的场景启用和禁用
        EditorBuildSettingsScene[] arrScene = EditorBuildSettings.scenes;
        for (int i = 0; i < arrScene.Length; i++)
        {
            if (arrScene[i].path.IndexOf("download", StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                arrScene[i].enabled = !CurrAssetLoadTarget.ToString().Equals("ASSETBUNDLE");
            }
        }
        EditorBuildSettings.scenes = arrScene;

        List<string> definesL = new List<string>();
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] defines);
        for (int i = 0; i < defines.Length; i++) definesL.Add(defines[i]);
        foreach (var item in Settings) definesL.Remove(item.Macro);
        AssetLoadTarget[] AssetLoadTargets = (AssetLoadTarget[])Enum.GetValues(typeof(AssetLoadTarget));
        foreach (var item in AssetLoadTargets) definesL.Remove(item.ToString());
        for (int i = 0; i < definesL.Count; i++) macor += string.Format("{0};", definesL[i]);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macor);
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
            //该字符串包含AssetLoadTargets[i]
            AssetLoadTarget[] AssetLoadTargets = (AssetLoadTarget[])Enum.GetValues(typeof(AssetLoadTarget));
            for (int i = 0; i < AssetLoadTargets.Length; i++)
            {
                if (m_Macor.IndexOf(AssetLoadTargets[i].ToString()) != -1)
                {
                    CurrAssetLoadTarget = AssetLoadTargets[i];
                }
            }

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


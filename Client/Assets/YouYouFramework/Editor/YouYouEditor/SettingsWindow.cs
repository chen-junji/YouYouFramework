using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;

/// <summary>
/// 开发模式管理窗口--SettingsWindow
/// </summary>
public class SettingsWindow : EditorWindow
{
	/// <summary>
	/// 该List的MacorItem是一条bool类型的选项卡
	/// </summary>
	private List<MacorItem> m_List = new List<MacorItem>();

	/// <summary>
	/// string表示选项卡名称;bool表示选项卡的选取状态√
	/// </summary>
	private Dictionary<string, bool> m_Dic = new Dictionary<string, bool>();

	/// <summary>
	/// 被设置为√状态的选项卡的Name
	/// </summary>
	private string m_Macor = null;

	/// <summary>
	/// 资源加载方式
	/// </summary>
	private string[] arrLoadTarget = { "RESOURCES", "ASSETBUNDLE", "EDITORLOAD" };
	private int LoadTargetIndex = 0;

	/// <summary>
	/// 结构方法
	/// </summary>
	void OnEnable()
	{
		//初始化m_Macor
		m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

		//==================初始化List==================
		m_List.Clear();
		m_List.Add(new MacorItem() { Name = "DEBUG_LOG_NORMAL", DisplayName = "打印普通日志", IsDebug = true, IsRelease = false });
		m_List.Add(new MacorItem() { Name = "DEBUG_LOG_PROCEDURE", DisplayName = "打印流程日志", IsDebug = true, IsRelease = false });
		m_List.Add(new MacorItem() { Name = "DEBUG_LOG_RESOURCE", DisplayName = "打印资源日志", IsDebug = true, IsRelease = false });
		m_List.Add(new MacorItem() { Name = "DEBUG_LOG_PROTO", DisplayName = "打印通讯日志", IsDebug = true, IsRelease = false });
		m_List.Add(new MacorItem() { Name = "DEBUG_LOG_ERROR", DisplayName = "打印错误日志", IsDebug = true, IsRelease = false });

		//===================初始化Dic===================
		for (int i = 0; i < m_List.Count; i++)
		{
			if (!string.IsNullOrEmpty(m_Macor) && m_Macor.IndexOf(m_List[i].Name) != -1)//m_Macor不为空,且该字符串包含m_List[i].Name
			{
				m_Dic[m_List[i].Name] = true;
			}
			else
			{
				m_Dic[m_List[i].Name] = false;
			}
		}
		for (int i = 0; i < arrLoadTarget.Length; i++)
		{
			if (!string.IsNullOrEmpty(m_Macor) && m_Macor.IndexOf(arrLoadTarget[i]) != -1)//m_Macor不为空,且该字符串包含arrLoadTarget[i]
			{
				m_Dic[arrLoadTarget[i]] = true;
				LoadTargetIndex = i;
			}
			else
			{
				m_Dic[arrLoadTarget[i]] = false;
			}
		}
	}

	void OnGUI()
	{
		//生成窗口以及各个bool选项
		for (int i = 0; i < m_List.Count; i++)
		{
			EditorGUILayout.BeginHorizontal("box");//开启一行位置
			m_Dic[m_List[i].Name] = GUILayout.Toggle(m_Dic[m_List[i].Name], m_List[i].DisplayName);
			EditorGUILayout.EndHorizontal();
		}
		//选择资源加载方式
		int selectLoadTargetIndex = EditorGUILayout.Popup(LoadTargetIndex, arrLoadTarget, GUILayout.Width(100));
		if (selectLoadTargetIndex != LoadTargetIndex)
		{
			LoadTargetIndex = selectLoadTargetIndex;
			EditorApplication.delayCall = () =>
			{
				for (int i = 0; i < arrLoadTarget.Length; i++)
				{
					m_Dic[arrLoadTarget[i]] = false;
				}
				m_Dic[arrLoadTarget[LoadTargetIndex]] = true;
			};
		}


		EditorGUILayout.BeginHorizontal();//开启一行位置
										  //生成"保存"按钮
		if (GUILayout.Button("保存", GUILayout.Width(100)))
		{
			SaveMacor();
		}
		EditorGUILayout.EndHorizontal();
	}
	/// <summary>
	/// 保存设置
	/// </summary>
	private void SaveMacor()
	{
		bool sceneEnabled;
		m_Macor = string.Empty;
		foreach (var item in m_Dic)//遍历选项卡Dic
		{
			if (item.Value)//如果该Dic的某个选项卡为true
			{
				m_Macor += string.Format("{0};", item.Key);
			}

			if (item.Key.Equals("ASSETBUNDLE"))
			{
				sceneEnabled = !item.Value;

				//设置BuildSetting中的场景启用和禁用
				EditorBuildSettingsScene[] arrScene = EditorBuildSettings.scenes;
				for (int i = 0; i < arrScene.Length; i++)
				{
					if (arrScene[i].path.IndexOf("download", StringComparison.CurrentCultureIgnoreCase) > -1)
					{
						arrScene[i].enabled = sceneEnabled;
					}
				}
				EditorBuildSettings.scenes = arrScene;
			}
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, m_Macor);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, m_Macor);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, m_Macor);
	}

	/// <summary>
	/// 宏项目
	/// </summary>
	public class MacorItem
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string Name;
		/// <summary>
		/// 显示的名称
		/// </summary>
		public string DisplayName;

		/// <summary>
		/// 是否调试项
		/// </summary>
		public bool IsDebug;

		/// <summary>
		/// 是否发布
		/// </summary>
		public bool IsRelease;
	}
}

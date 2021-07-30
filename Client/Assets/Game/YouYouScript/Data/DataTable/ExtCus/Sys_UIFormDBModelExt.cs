using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public partial class Sys_UIFormDBModel
	{
		private Dictionary<string, Sys_UIFormEntity> m_NameByEntityDic;

		private void InitNameByEntityDic()
		{
			m_NameByEntityDic = new Dictionary<string, Sys_UIFormEntity>();
			for (int i = 0; i < m_List.Count; i++)
			{
				Sys_UIFormEntity sys_UIForm = m_List[i];

				string assetPath = string.Empty;
				switch (GameEntry.CurrLanguage)
				{
					case YouYouLanguage.Chinese:
						assetPath = sys_UIForm.AssetPath_Chinese;
						break;
					case YouYouLanguage.English:
						assetPath = string.IsNullOrWhiteSpace(sys_UIForm.AssetPath_English) ? sys_UIForm.AssetPath_Chinese : sys_UIForm.AssetPath_English;
						break;
					default:
						assetPath = sys_UIForm.AssetPath_Chinese;
						break;
				}

				string[] strs = assetPath.Split('/');
				if (strs.Length >= 1)
				{
					string str = strs[strs.Length - 1];
					if (m_NameByEntityDic.ContainsKey(str))
					{
						//Debug.Log("UI_Form OpenFail path == " + str);
					}
					else
					{
						m_NameByEntityDic.Add(str, sys_UIForm);
					}
				}
			}
		}
		public int GetIdByName(string name)
		{
			if (m_NameByEntityDic == null) InitNameByEntityDic();

			if (m_NameByEntityDic.ContainsKey(name))
			{
				return m_NameByEntityDic[name].Id;
			}
			Debug.LogError("没有找到Prefab, name==" + name);

			return -1;
		}
	}
}
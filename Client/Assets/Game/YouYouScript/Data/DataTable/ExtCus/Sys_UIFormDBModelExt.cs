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
				Sys_UIFormEntity entity = m_List[i];

				string path = string.Empty;
				switch (GameEntry.CurrLanguage)
				{
					case YouYouLanguage.Chinese:
						path = entity.AssetPath_Chinese;
						break;
					case YouYouLanguage.English:
						if (entity.AssetPath_English != null)
						{
							path = entity.AssetPath_English;
						}
						else
						{
							path = entity.AssetPath_Chinese;
						}
						break;
				}
				string[] strs = path.Split('/');
				if (strs.Length >= 1)
				{
					string str = strs[strs.Length - 1];
					if (m_NameByEntityDic.ContainsKey(str))
					{
						//Debug.Log("UI_Form OpenFail path == " + str);
					}
					else
					{
						m_NameByEntityDic.Add(str, entity);
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
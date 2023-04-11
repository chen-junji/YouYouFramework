using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_UIFormDBModel
    {
        public Dictionary<string, Sys_UIFormEntity> NameByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            NameByDic = new Dictionary<string, Sys_UIFormEntity>();
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
                        path = string.IsNullOrWhiteSpace(entity.AssetPath_English) ? entity.AssetPath_Chinese : entity.AssetPath_English;
                        break;
                }
                entity.AssetFullName = string.Format("UI/UIPrefab/{0}.prefab", path).ToString();
                string[] strs = path.Split('/');
                if (strs.Length >= 1)
                {
                    string str = strs[strs.Length - 1];
                    if (NameByDic.ContainsKey(str))
                    {
                        GameEntry.LogError(LogCategory.Framework, "UIForm名称有重复! ==" + str);
                    }
                    else
                    {
                        NameByDic.Add(str, entity);
                    }
                }
            }
        }

        public int GetIdByName(string name)
        {
            if (NameByDic.ContainsKey(name))
            {
                return NameByDic[name].Id;
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "没有找到Prefab, name==" + name);

            return -1;
        }
    }
}
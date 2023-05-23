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

                switch (GameEntry.CurrLanguage)
                {
                    case YouYouLanguage.Chinese:
                        entity.AssetFullName = entity.AssetPath_Chinese;
                        break;
                    case YouYouLanguage.English:
                        entity.AssetFullName = string.IsNullOrWhiteSpace(entity.AssetPath_English) ? entity.AssetPath_Chinese : entity.AssetPath_English;
                        break;
                }
                string[] strs = entity.AssetFullName.Split('.')[0].Split('/');
                if (strs.Length >= 1)
                {
                    string str = strs[strs.Length - 1];
                    if (NameByDic.ContainsKey(str))
                    {
                        GameEntry.LogError(LogCategory.Framework, "名称有重复! ==" + str);
                    }
                    else
                    {
                        NameByDic.Add(str, entity);
                    }
                }
            }
        }

        public Sys_UIFormEntity GetEntity(string name)
        {
            if (NameByDic.ContainsKey(name))
            {
                return NameByDic[name];
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "没有找到资源, Name==" + name);
            return null;
        }
    }
}
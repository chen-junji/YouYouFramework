using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
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

                string[] strs = entity.AssetFullPath.Split('.')[0].Split('/');
                if (strs.Length >= 1)
                {
                    string str = strs[strs.Length - 1];
                    if (NameByDic.ContainsKey(str))
                    {
                        GameEntry.LogError(LogCategory.Framework, "名称有重复! ==" + str);
                    }
                    else
                    {
                        entity.UIFromName = str;
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
            YouYouFramework.GameEntry.LogError(LogCategory.Framework, "没有找到资源, Name==" + name);
            return null;
        }
    }
}
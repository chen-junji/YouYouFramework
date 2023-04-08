using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_PrefabDBModel
    {
        private Dictionary<string, Sys_PrefabEntity> m_NameByEntityDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            m_NameByEntityDic = new Dictionary<string, Sys_PrefabEntity>();

            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_PrefabEntity entity = m_List[i];
                string[] strs = entity.AssetPath.Split('/');
                string name = strs[strs.Length - 1];
                if (m_NameByEntityDic.ContainsKey(name))
                {
                    GameEntry.LogError("Sys_Prefab有名称重复! == " + entity.AssetPath);
                }
                else
                {
                    m_NameByEntityDic.Add(name, entity);
                }
#if !RESOURCES
                entity.AssetPath = entity.AssetPath + entity.Suffix;
#endif
            }
        }


        public Sys_PrefabEntity GetEntityByName(string name)
        {
            Sys_PrefabEntity sys_Prefab = null;
            m_NameByEntityDic.TryGetValue(name, out sys_Prefab);
            return sys_Prefab;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_AudioDBModel
    {
        private Dictionary<string, Sys_AudioEntity> m_NameByEntityDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            m_NameByEntityDic = new Dictionary<string, Sys_AudioEntity>();

            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_AudioEntity entity = m_List[i];
                if (entity.Priority == 0) entity.Priority = 128;
                entity.AssetPath = "Audio/" + entity.AssetPath;

                string[] strs = entity.AssetPath.Split('/');
                string name = strs[strs.Length - 1];
                if (m_NameByEntityDic.ContainsKey(name))
                {
                    GameEntry.LogWarning(LogCategory.Audio, "Sys_Audio有名称重复! == " + entity.AssetPath);
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


        public Sys_AudioEntity GetEntity(string name)
        {
            Sys_AudioEntity sys_Prefab = null;
            m_NameByEntityDic.TryGetValue(name, out sys_Prefab);
            return sys_Prefab;
        }
    }
}
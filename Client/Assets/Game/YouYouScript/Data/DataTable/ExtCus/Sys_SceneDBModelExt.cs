using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_SceneDBModel
    {
        private Dictionary<string, List<Sys_SceneEntity>> GroupNameByDic = new Dictionary<string, List<Sys_SceneEntity>>();
        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_SceneEntity entity = m_List[i];
                string[] strs = entity.ScenePath.Split('/');
                if (strs.Length >= 1)
                {
                    if (!GroupNameByDic.ContainsKey(entity.SceneGroup))
                    {
                        GroupNameByDic.Add(entity.SceneGroup, new List<Sys_SceneEntity>());
                    }
                    GroupNameByDic[entity.SceneGroup].Add(entity);
                }
            }
        }

        public List<Sys_SceneEntity> GetListByGroupName(string groupName, int maxCount)
        {
            if (maxCount == -1) maxCount = GroupNameByDic[groupName].Count;
            List<Sys_SceneEntity> lst = new List<Sys_SceneEntity>();
            for (int i = 0; i < maxCount; i++)
            {
                lst.Add(GroupNameByDic[groupName][i]);
            }
            return lst;
        }
    }
}
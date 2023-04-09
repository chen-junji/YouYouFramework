using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_SceneDBModel
    {
        public List<Sys_SceneEntity> GetListBySceneName(string sceneName)
        {
            List<Sys_SceneEntity> retValue = new List<Sys_SceneEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                if (m_List[i].SceneName == sceneName)
                {
                    retValue.Add(m_List[i]);
                }
            }
            return retValue;
        }
    }
}
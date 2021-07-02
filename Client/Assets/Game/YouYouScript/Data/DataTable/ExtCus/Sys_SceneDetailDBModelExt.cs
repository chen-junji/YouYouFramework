using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_SceneDetailDBModel
    {
        public List<Sys_SceneDetailEntity> GetListBySceneId(string sceneName, int sceneGrade)
        {
            List<Sys_SceneDetailEntity> retList = new List<Sys_SceneDetailEntity>();

            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_SceneDetailEntity entity = m_List[i];
                if (entity.SceneName == sceneName && entity.SceneGrade <= sceneGrade)
                {
                    retList.Add(entity);
                }
            }
            return retList;
        }
    }
}
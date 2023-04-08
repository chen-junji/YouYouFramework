using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public partial class Sys_SceneDBModel
    {
        public int GetRangeSceneId()
        {
            List<Sys_SceneEntity> lst = new List<Sys_SceneEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_SceneEntity CurrSceneEntity = m_List[i];

                bool isDownload = true;
#if ASSETBUNDLE
            List<Sys_SceneDetailEntity> currSceneDetailList = GameEntry.DataTable.Sys_SceneDetailDBModel.GetListBySceneId(CurrSceneEntity.SceneName, 2);
            for (int j = 0; j < currSceneDetailList.Count; j++)
            {
                if (!GameEntry.Resource.ResourceManager.CheckVersionChangeSingle(currSceneDetailList[j].ScenePath))
                {
                    isDownload = false;
                }
            }
#endif
                if (isDownload) lst.Add(CurrSceneEntity);
            }

            return lst[Random.Range(0, lst.Count)].Id;
        }

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
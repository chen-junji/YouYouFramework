using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class Sys_PrefabDBModel
{
    private Dictionary<string, Sys_PrefabEntity> m_NameByEntityDic;

    private void InitNameByEntityDic()
    {
        m_NameByEntityDic = new Dictionary<string, Sys_PrefabEntity>();

        int len = m_List.Count;
        for (int i = 0; i < len; i++)
        {
            Sys_PrefabEntity entity = m_List[i];
            string[] strs = entity.AssetPath.Split('/');//�ָ�path
            string name = strs[strs.Length - 1];


			if (m_NameByEntityDic.ContainsKey(name))
            {
                Debug.LogError("Sys_Prefab����,����������ظ����Ƶ���! == " + entity.AssetPath);
            }
            else
            {
                m_NameByEntityDic.Add(name, entity);
            }
        }
    }
    public int GetPrefabIdByName(string name)
    {
        if (m_NameByEntityDic == null) InitNameByEntityDic();

        if (m_NameByEntityDic.ContainsKey(name))
        {
            return m_NameByEntityDic[name].Id;
        }
        return -1;
    }
}

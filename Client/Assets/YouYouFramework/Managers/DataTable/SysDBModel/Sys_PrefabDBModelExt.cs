using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public partial class Sys_PrefabDBModel
{
    //private Dictionary<string, Sys_PrefabEntity> m_NameByEntityDic;

    //private void InitNameByEntityDic()
    //{
    //    m_NameByEntityDic = new Dictionary<string, Sys_PrefabEntity>();

    //    int len = m_List.Count;
    //    for (int i = 0; i < len; i++)
    //    {
    //        Sys_PrefabEntity entity = m_List[i];
    //        string[] strs = entity.AssetPath.Split('/');//分割path
    //        string[] names = strs[strs.Length - 1].Split('.');//分割xxx.prefab

    //        if (m_NameByEntityDic.ContainsKey(names[0]))
    //        {
    //            Debug.LogError("Sys_Prefab表中,不允许出现重复名称的项! == " + entity.AssetPath);
    //        }
    //        else
    //        {
    //            m_NameByEntityDic.Add(names[0], entity);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 根据Prefab名称返回Id
    ///// </summary>
    //public int GetPrefabIdByName(string name)
    //{
    //    if (m_NameByEntityDic == null) InitNameByEntityDic();

    //    if (m_NameByEntityDic.ContainsKey(name))
    //    {
    //        return m_NameByEntityDic[name].Id;
    //    }

    //    Debug.LogError("Prefab名称错误,查找不到!!!");
    //    return -1;
    //}
}

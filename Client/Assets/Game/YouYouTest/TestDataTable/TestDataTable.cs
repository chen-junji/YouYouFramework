using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestDataTable : MonoBehaviour
{
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //根据ID获取某一行配置数据
            Sys_UIFormEntity entity = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(1);
            GameEntry.Log(LogCategory.ZhangSan, entity.ToJson());
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //获取所有配置数据
            List<Sys_UIFormEntity> lst = GameEntry.DataTable.Sys_UIFormDBModel.GetList();
            GameEntry.Log(LogCategory.ZhangSan, lst.ToJson());
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            //根据名字获取配置数据
            Sys_UIFormEntity entity = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity("FormDialog"); ;
            GameEntry.Log(LogCategory.ZhangSan, entity.ToJson());
        }
    }
#endif
}

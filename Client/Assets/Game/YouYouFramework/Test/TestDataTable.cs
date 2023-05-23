using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestDataTable : MonoBehaviour
{
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            Sys_UIFormEntity entity = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(1);
            GameEntry.Log(LogCategory.ZhangSan, entity.ToJson());
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            List<Sys_UIFormEntity> lst = GameEntry.DataTable.Sys_UIFormDBModel.GetList();
            GameEntry.Log(LogCategory.ZhangSan, lst.ToJson());
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            Sys_UIFormEntity entity = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity("FormDialog"); ;
            GameEntry.Log(LogCategory.ZhangSan, entity.ToJson());
        }
    }
#endif
}

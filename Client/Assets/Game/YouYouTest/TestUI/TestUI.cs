using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //方式1, 不带初始化
            GameEntry.UI.OpenUIForm<DialogForm>();
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            //方式2, 带初始化
            //DialogForm.ShowForm("框架内部流程全部加载完毕, 已经进入登录流程", "登录流程");
            if (GameEntry.DataTable.Sys_DialogDBModel.keyDic.TryGetValue("LoadingOK", out var entity1))
            {
                DialogForm.ShowFormByKey("LoadingOK");
            }
        }
    }
}
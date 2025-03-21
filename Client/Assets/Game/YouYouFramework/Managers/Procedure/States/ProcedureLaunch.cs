using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using YouYouFramework;


/// <summary>
/// 启动流程
/// </summary>
public class ProcedureLaunch : ProcedureBase
{
    private string[] permissions = new string[]
    {
        "android.permission.WRITE_EXTERNAL_STORAGE"
    };
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        //获取安卓权限
        permissions.ToList().ForEach(s =>
        {
            //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
        });

        GameEntry.Procedure.ChangeState(ProcedureManager.EState.Preload);
    }
}
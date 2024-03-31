using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using YouYouMain;

namespace YouYouFramework
{
    /// <summary>
    /// 启动流程
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        private string[] permissions = new string[]
        {
            "android.permission.WRITE_EXTERNAL_STORAGE"
        };
        internal override void OnEnter()
        {
            base.OnEnter();
            //获取安卓权限
            permissions.ToList().ForEach(s =>
            {
                //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });

            if (MainEntry.IsAssetBundleMode)
            {
                GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
            }
            else
            {
                GameEntry.Procedure.ChangeState(ProcedureState.Preload);
            }
        }
    }
}
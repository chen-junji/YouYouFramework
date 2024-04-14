using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

/// <summary>
/// 游戏主流程
/// </summary>
public class ProcedureMain : ProcedureBase
{
    internal override void OnEnter()
    {
        base.OnEnter();
        GameEntry.UI.OpenUIForm<LoadingForm>();
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Main);
        GameEntry.Input.SetEnable(true);
    }
    internal override void OnLeave()
    {
        base.OnLeave();

        //退出登录时, 清空业务数据
        GameEntry.Model.Clear();
        GameEntry.Input.SetEnable(false);
    }
}

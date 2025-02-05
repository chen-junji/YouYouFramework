using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;
using YouYouMain;

/// <summary>
/// 游戏主流程
/// </summary>
public class ProcedureMain : ProcedureBase
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        GameEntry.UI.OpenUIForm<LoadingForm>();
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Main);
    }
    public override void OnLeave(int newState)
    {
        base.OnLeave(newState);
        //退出登录时, 清空业务数据
        GameEntry.Model.Clear();
    }
}

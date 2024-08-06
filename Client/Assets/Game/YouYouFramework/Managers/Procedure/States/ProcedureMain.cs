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

        GameEntry.Input.SetEnable(true);
#if UNITY_EDITOR
        GameEntry.Input.CurrFsm.SetTrigger(MainEntry.ParamsSettings.MobileDebug ? InputManager.ParamConst.TriggerTouch : InputManager.ParamConst.TriggerKeyboard);
#elif UNITY_STANDALONE
        GameEntry.Input.CurrFsm.SetTrigger(InputManager.ParamConst.TriggerKeyboard);
#elif UNITY_IOS || UNITY_ANDROID
        GameEntry.Input.CurrFsm.SetTrigger(InputManager.ParamConst.TriggerTouch);
#endif

    }
    public override void OnLeave(int newState)
    {
        base.OnLeave(newState);
        //退出登录时, 清空业务数据
        GameEntry.Model.Clear();
        GameEntry.Input.SetEnable(false);
    }
}

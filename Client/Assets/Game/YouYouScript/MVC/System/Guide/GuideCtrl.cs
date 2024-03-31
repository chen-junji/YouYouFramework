using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


public enum GuideState
{
    /// <summary>
    /// 未触发引导
    /// </summary>
    None,
    /// <summary>
    /// 第一关,局外
    /// </summary>
    Battle1,
    /// <summary>
    /// 结束
    /// </summary>
    Finish
}
/// <summary>
/// 新手引导
/// </summary>
public class GuideCtrl : Singleton<GuideCtrl>
{
    public GuideState CurrentState { get; private set; }       //当前处于哪个状态

    public event Action<GuideState, GuideState> OnStateChange;

    /// <summary>
    /// 触发下一步
    /// </summary>
    public event Action OnNextOne;

    public GuideGroup GuideGroup;


    public bool OnStateEnter(GuideState state)
    {
        if (MainEntry.ParamsSettings.ActiveGuide == false) return false;

        if (CurrentState == state) return false;

        switch (state)
        {
            //只触发一次的引导
            case GuideState.Battle1:
                if (GameEntry.Model.GetModel<GuideModel>().NextGuide != state) return false;
                break;

            //每次引导结束
            case GuideState.None:
                GuideGroup = null;
                break;
        }

        OnStateChange?.Invoke(CurrentState, state);
        CurrentState = state;
        return true;
    }

    public bool NextGroup(GuideState descGroup)
    {
        if (MainEntry.ParamsSettings.ActiveGuide == false) return false;
        if (CurrentState != descGroup) return false;

        //完成当前任务
        if (OnNextOne != null)
        {
            Action onNextOne = OnNextOne;
            OnNextOne = null;
            onNextOne();
        }
        GuideGroup.TaskGroup.LeaveCurrTask();

        //GameEntry.Log(LogCategory.Hollow, "NextGroup:" + descGroup);
        return true;
    }
}

public class GuideEntity
{
    //当前已完成的新手引导
    public GuideState CurrGuide;
}
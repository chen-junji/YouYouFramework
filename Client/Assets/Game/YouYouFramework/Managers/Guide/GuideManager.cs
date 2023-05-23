using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using System;
using YouYou;
using System.Threading.Tasks;
using UnityEngine.UI;


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
/// 新手引导管理器
/// </summary>
public class GuideManager
{
    public GuideState CurrentState { get; private set; }       //当前处于哪个状态

    public event Action<GuideState, GuideState> OnStateChange;

    /// <summary>
    /// 触发下一步
    /// </summary>
    public Action OnNextOne { get { return m_OnNextOne; } set { m_OnNextOne = value; } }
    public Action m_OnNextOne;

    public GuideGroup GuideGroup { get; private set; }


    public bool OnStateEnter(GuideState state)
    {
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;

        if (CurrentState == state) return false;

        switch (state)
        {
            //只触发一次的引导
            case GuideState.Battle1:
                if (GameEntry.Data.GuideDataMgr.NextGuide != state) return false;
                break;

            //每次引导结束
            case GuideState.None:
                GameEntry.Guide.GuideGroup = null;
                break;
        }

        OnStateChange?.Invoke(CurrentState, state);
        CurrentState = state;
        return true;
    }

    public bool NextGroup(GuideState descGroup)
    {
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
        if (CurrentState != descGroup) return false;

        GuideUtil.CloseHollow();
        if (OnNextOne != null)
        {
            Action onNextOne = OnNextOne;
            OnNextOne = null;
            onNextOne();
        }

        //GameEntry.Log(LogCategory.Hollow, "NextGroup:" + descGroup);
        return true;
    }


    #region 条件触发判断
    //第一关 新手引导
    public void EnterBattle1()
    {
        if (!GameEntry.Guide.OnStateEnter(GuideState.Battle1)) return;

        GuideGroup = new GuideGroup();
        GuideGroup.AddGuide(() =>
        {
            //穿透点击按钮, 触发下一步
            GuideUtil.ShowOrNextHollow();
        });
        GuideGroup.AddGuide(() =>
        {
            //点击全屏遮罩, 触发下一步
            GuideUtil.ShowOrNextHollow();
        });
        GuideGroup.AddGuide(() =>
        {
            //监听按钮点击, 触发下一步
            Button button = null;
            GuideUtil.CheckBtnNext(button);
        });
        GuideGroup.AddGuide(() =>
        {
            //监听开关打开, 触发下一步
            Toggle toggle = null;
            GuideUtil.CheckToggleNext(toggle);
        });
        GuideGroup.AddGuide(() =>
        {
            //监听事件 触发下一步
            GuideUtil.CheckEventNext("EventName");
        });
        GuideGroup.Run(()=>
        {
            GameEntry.Data.GuideDataMgr.GuideCompleteOne(CurrentState);
        });
    }
    #endregion

}

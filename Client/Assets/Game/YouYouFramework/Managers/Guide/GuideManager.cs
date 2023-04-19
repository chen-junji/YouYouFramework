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
    //单机或联网
    public bool b_native = false;

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
                if (GetNextNetState() != state) return false;
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
        //if (GameEntry.ParamsSettings.GetGradeParamData("EditorGuide") == 0 && Application.installMode == ApplicationInstallMode.Editor) return false;
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

    /// <summary>
    ///当一个完成后的逻辑 By Sixalloy
    /// </summary>
    public void GuideCompleteOne(GuideState currentState)
    {
        GameEntry.Log(LogCategory.Guide, "GuideCompleteOne:");

        //只能保存后面的引导
        if (currentState >= GetNextNetState())
        {
            GameEntry.PlayerPrefs.Data.GuideEntity.GuideCompleteOne(currentState);
            //这里可以网络存档
            GameEntry.Log(LogCategory.Guide, "GuideCompleteOne:" + currentState.ToString() + currentState.ToInt());
        }
    }

    private GuideState GetNextNetState()
    {
        if (b_native) return GameEntry.PlayerPrefs.Data.GuideEntity.CurrGuide + 1;
        return GameEntry.PlayerPrefs.Data.GuideEntity.CurrGuide + 1;//这里可以网络存档
    }


    #region 条件触发判断
    //第一关局外
    public void EnterBattle1()
    {
        if (!GameEntry.Guide.OnStateEnter(GuideState.Battle1)) return;

        GuideGroup = new GuideGroup();
        //主界面, 对话
        GuideGroup.AddGuide(2, () =>
        {
            GameEntry.Audio.PlayBGM("BGM");
            GameEntry.Log(LogCategory.Guide, "播放BGM-HOLLOW");
            GuideUtil.ShowOrNextHollow();
        });
        GuideGroup.Run();

    }
    #endregion

}

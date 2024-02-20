using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;


public enum GuideState
{
    /// <summary>
    /// δ��������
    /// </summary>
    None,
    /// <summary>
    /// ��һ��,����
    /// </summary>
    Battle1,
    /// <summary>
    /// ����
    /// </summary>
    Finish
}
/// <summary>
/// ��������
/// </summary>
public class GuideCtrl : Singleton<GuideCtrl>
{
    public GuideState CurrentState { get; private set; }       //��ǰ�����ĸ�״̬

    public event Action<GuideState, GuideState> OnStateChange;

    /// <summary>
    /// ������һ��
    /// </summary>
    public event Action OnNextOne;

    public GuideGroup GuideGroup;


    public bool OnStateEnter(GuideState state)
    {
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;

        if (CurrentState == state) return false;

        switch (state)
        {
            //ֻ����һ�ε�����
            case GuideState.Battle1:
                if (GameEntry.Model.GetModel<GuideModel>().NextGuide != state) return false;
                break;

            //ÿ����������
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
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
        if (CurrentState != descGroup) return false;

        //��ɵ�ǰ����
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
    //��ǰ����ɵ���������
    public GuideState CurrGuide;
}
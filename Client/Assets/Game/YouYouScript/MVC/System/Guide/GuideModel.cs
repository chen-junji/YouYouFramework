using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;
using System;

public class GuideModel
{
    public enum GuideEventEnum
    {
        TestEvent1
    }

    public Action<GuideEventEnum> ActionGuide;

    //单机或网络存档
    private bool b_native = true;

    private GuideEntity GuideEntity;

    public GuideModel()
    {
        GameEntry.Event.AddEventListener(CommonEventId.GameEntryOnApplicationQuit, SaveDataAll);
        GameEntry.Event.AddEventListener(CommonEventId.GameEntryOnApplicationPause, SaveDataAll);

        if (b_native)
        {
            GuideEntity = GameEntry.PlayerPrefs.GetObject<GuideEntity>("GuideEntity");
        }
        else
        {
            //这里可以改成网络存档
        }
    }

    public void SaveDataAll(object userData)
    {
        if (b_native)
        {
            GameEntry.PlayerPrefs.SetObject("GuideEntity", GuideEntity);
        }
        else
        {
            //这里可以改成网络存档
        }
    }

    public GuideState NextGuide { get { return GuideEntity.CurrGuide + 1; } }


    /// <summary>
    /// 新手引导 完成1个模块 存档
    /// </summary>
    public void GuideCompleteOne(GuideState guideState)
    {
        //只能保存后面的引导
        if (guideState >= GuideEntity.CurrGuide + 1)
        {
            GuideEntity.CurrGuide = guideState;
            GameEntry.Log(LogCategory.Guide, "GuideCompleteOne:" + guideState.ToString() + guideState.ToInt());
        }
    }
}
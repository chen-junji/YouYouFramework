using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class GuideDataMgr : DataMgrBase<GuideDataMgr.EventName>
{
    public enum EventName
    {

    }
    //单机或网络存档
    private bool b_native = true;

    private GuideEntity GuideEntity;

    public void Init()
    {
        if (b_native)
        {
            GuideEntity = GameEntry.Data.PlayerPrefsDataMgr.GetObject<GuideEntity>("GuideEntity");
        }
        else
        {
            //这里可以改成网络存档
        }
    }
    public void SaveDataAll()
    {
        if (b_native)
        {
            GameEntry.Data.PlayerPrefsDataMgr.SetObject("GuideEntity", GuideEntity);
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

public class GuideEntity
{
    //当前已完成的新手引导
    public GuideState CurrGuide;
}
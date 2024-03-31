using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "加载"界面
/// </summary>
public partial class LoadingForm : UIFormBase
{
    private void OnLoadingProgressChange(object userData)
    {
        VarFloat varFloat = (VarFloat)userData;
        m_Txt_Tip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(varFloat * 100));
        m_Sbar_Progress.size = varFloat;

        if (varFloat == 1)
        {
            Close();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(CommonEventId.LoadingSceneUpdate, OnLoadingProgressChange);

        //m_Txt_Tip.text = string.Empty;
        //m_Sbar_Progress.size = 0;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(CommonEventId.LoadingSceneUpdate, OnLoadingProgressChange);
    }
}

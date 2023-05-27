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
public class FormLoading : UIFormBase
{
    [SerializeField]
    private Scrollbar m_Scrollbar;
    [SerializeField]
    private Text txtTip;

    private void OnLoadingProgressChange(object userData)
    {
        float baseParams = (float)userData;
        txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(baseParams * 100));
        m_Scrollbar.size = baseParams;

        if (baseParams == 1) Close();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //txtTip.text = string.Empty;
        //m_Scrollbar.size = 0;
        MainEntry.Data.AddEventListener(SysDataMgr.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
}

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


    protected override void Awake()
    {
        base.Awake();
        MainEntry.Data.AddEventListener(SysDataMgr.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
    private void OnLoadingProgressChange(object userData)
    {
        float baseParams = (float)userData;
        float progress = Math.Min(baseParams * 100, 100);
        txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(progress));
        m_Scrollbar.size = baseParams;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //txtTip.text = string.Empty;
        //m_Scrollbar.size = 0;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
}

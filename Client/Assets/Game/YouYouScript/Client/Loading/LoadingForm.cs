using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;


/// <summary>
/// "加载"界面
/// </summary>
public partial class LoadingForm : UIFormBase
{
    [SerializeField] private Scrollbar m_Sbar_Progress;
    [SerializeField] private Text m_Txt_Tip;

    private void OnLoadingProgressChange(float varFloat)
    {
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
        GameEntry.Scene.LoadingUpdateAction += OnLoadingProgressChange;

        //m_Txt_Tip.text = string.Empty;
        //m_Sbar_Progress.size = 0;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Scene.LoadingUpdateAction -= OnLoadingProgressChange;
    }
}

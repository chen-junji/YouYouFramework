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
		MainEntry.Data.AddEventListener(SysDataMgr.EventName.LOADING_SCENE_UPDATE, OnLoadingProgressChange);
	}
    private void OnLoadingProgressChange(object userData)
	{
		BaseParams baseParams = (BaseParams)userData;

		float progress = Math.Min(baseParams.FloatParam1 * 100, 100);
		if (baseParams.IntParam1 == 0)
		{
			//txtTip.text = GameEntry.Localization.GetString("Loading.ChangeScene", Math.Floor(progress));
			txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(progress));
		}
		m_Scrollbar.size = baseParams.FloatParam1;
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
		MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.LOADING_SCENE_UPDATE, OnLoadingProgressChange);
	}
}

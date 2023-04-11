using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "加载"界面
/// </summary>
public class LoadingForm : UIFormBase
{
	[SerializeField]
	private Scrollbar m_Scrollbar;
	[SerializeField]
	private Text txtTip;


	protected override void OnInit(object userData)
	{
		base.OnInit(userData);
		GameEntry.Event.Common.AddEventListener(CommonEventId.LoadingProgressChange, OnLoadingProgressChange);
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

	protected override void OnOpen(object userData)
	{
		base.OnOpen(userData);
		//txtTip.text = string.Empty;
		//m_Scrollbar.size = 0;
	}
	protected override void OnClose()
	{
		base.OnClose();
		GameEntry.Event.Common.RemoveEventListener(CommonEventId.LoadingProgressChange, OnLoadingProgressChange);
	}
	protected override void OnBeforDestroy()
	{
		base.OnBeforDestroy();
	}
}

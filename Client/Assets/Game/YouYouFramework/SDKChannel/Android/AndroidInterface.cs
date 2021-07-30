using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class AndroidInterface : MonoBehaviour
{
	public static AndroidInterface Instance;

	private AndroidJavaClass jc;
	private AndroidJavaObject jo;

	#region 当前的安卓SDK
	private IAndroidSDK currAndroidSDK;
	public IAndroidSDK GetCurrAndroidSDK()
	{
		InitAndroidSDK();
		return currAndroidSDK;
	}
	#endregion

	private bool m_IsInitAndroidSDK;
	private void InitAndroidSDK()
	{
		if (m_IsInitAndroidSDK) return;
		switch (GameEntry.Data.SysData.CurrChannelConfig.ChannelId)
		{
			case 146:
				currAndroidSDK = new SDK_YouYou();
				break;
		}
		m_IsInitAndroidSDK = true;
	}

	void Awake()
	{
		Instance = this;

#if UNITY_ANDROID && !UNITY_EDITOR
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
#endif
	}

	/// <summary>
	/// 执行安卓动作
	/// </summary>
	/// <param name="actionName">动作名称</param>
	/// <param name="param">动作参数</param>
	public void DoAndroidAction(string actionName, string param)
	{
		if (jo != null)
		{
			jo.Call("DoAndroidAction", new object[] { actionName, param });
		}
	}

	/// <summary>
	/// 执行Unity动作
	/// </summary>
	/// <param name="userData"></param>
	private void DoUnityAction(string userData)
	{
		string[] arr = userData.Split('^');
		string actionName = arr[0];
		string param = arr[1];

		InitAndroidSDK();
		GetCurrAndroidSDK().DoAction(actionName, param, null);
	}
}
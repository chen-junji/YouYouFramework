using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif


/// <summary>
/// 震动管理器
/// </summary>
public class TapEngine
{
    public bool IsShake { get; private set; }

    public void Init()
    {
        GameEntry.Data.PlayerPrefsDataMgr.SetBoolHas(PlayerPrefsDataMgr.EventName.IsShake, true);
        GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.IsShake, RefreshIsShake);
        RefreshIsShake(null);
    }
    private void RefreshIsShake(object p)
    {
        IsShake = GameEntry.Data.PlayerPrefsDataMgr.GetBool(PlayerPrefsDataMgr.EventName.IsShake);
    }

    #region 苹果
#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
    [DllImport ("__Internal")]
    private static extern void _TAG_Unity_iOSTapticNotification (int typ);

    [DllImport ("__Internal")]
    private static extern void _TAG_Unity_iOSTapticSelection ();

    [DllImport ("__Internal")]
    private static extern void _TAG_Unity_iOSTapticImpact (int style);

    [DllImport ("__Internal")]
    private static extern bool _TAG_Unity_iOSTapticIsSupport ();
#endif

    public void Selection()
    {
        if (!IsShake) return;
#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
        _TAG_Unity_iOSTapticSelection ();
#endif
    }

    public void Impact(int style)
    {
        if (!IsShake) return;
#if (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
       _TAG_Unity_iOSTapticImpact (style);
#endif
    }
    #endregion

    #region 安卓
    public void Vibrate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (IsAndroid()) Handheld.Vibrate();
#endif
    }
    private bool IsAndroid()
    {
        if (!IsShake) return false;
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
        return false;
#endif
    }
    #endregion
}
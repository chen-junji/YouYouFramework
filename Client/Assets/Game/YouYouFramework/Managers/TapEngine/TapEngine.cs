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
    public bool IsShake = true;

    public void Init()
    {
        RefreshIsShake(null);
        GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.IsShake, RefreshIsShake);
    }
    private void RefreshIsShake(object p)
    {
        IsShake = GameEntry.Data.PlayerPrefs.GetLoggerDic(CommonEventId.IsShake).ToInt() == 1;
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
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif
    public void Vibrate()
    {
        if (isAndroid())
            vibrator.Call("vibrate");
        else
            Handheld.Vibrate();
    }
    public void Vibrate(long milliseconds)
    {
        if (isAndroid())
            vibrator.Call("vibrate", milliseconds);
        else
            Handheld.Vibrate();
    }
    public void Vibrate(long[] pattern, int repeat)
    {
        if (isAndroid())
            vibrator.Call("vibrate", pattern, repeat);
        else
            Handheld.Vibrate();
    }

    public void Cancel()
    {
        if (isAndroid())
            vibrator.Call("cancel");
    }

    private bool isAndroid()
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
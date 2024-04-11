using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class QualityModel
{
    public Action ScreenChange;
    public Action QualityLevelChange;
    public Action FrameRateChange;

    public enum EventId
    {
        QualityLevel,
        Screen,
        FrameRate
    }

    public enum Quality
    {
        Low,
        Medium,
        High
    }
    public enum FrameRate
    {
        Low,
        Medium,
        High,
    }
    public enum ScreenLevel
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// 画面品质
    /// </summary>
    public Quality CurrQuality { get; private set; }
    /// <summary>
    /// 分辨率
    /// </summary>
    public ScreenLevel CurrScreen { get; private set; }
    /// <summary>
    /// 限制游戏帧数,FPS
    /// </summary>
    public FrameRate CurrFrameRate { get; private set; }


    public QualityModel()
    {
        GameEntry.PlayerPrefs.SetIntHas(PlayerPrefsConstKey.FrameRate, 2);
        GameEntry.PlayerPrefs.SetIntHas(PlayerPrefsConstKey.Screen, 2);
        GameEntry.PlayerPrefs.SetIntHas(PlayerPrefsConstKey.QualityLevel, 2);

        //初始画质设置
        SetQuality((Quality)GameEntry.PlayerPrefs.GetInt(PlayerPrefsConstKey.QualityLevel));
        SetScreen((ScreenLevel)GameEntry.PlayerPrefs.GetInt(PlayerPrefsConstKey.Screen));
        SetFrameRate((FrameRate)GameEntry.PlayerPrefs.GetInt(PlayerPrefsConstKey.FrameRate));
    }

    public void SetQuality(Quality quality)
    {
        CurrQuality = quality;
        QualitySettings.SetQualityLevel(CurrQuality.ToInt(), true);
        GameEntry.PlayerPrefs.SetInt(PlayerPrefsConstKey.QualityLevel, (int)quality);
        QualityLevelChange?.Invoke();
    }
    public void SetScreen(ScreenLevel value)
    {
        CurrScreen = value;
        GameEntry.PlayerPrefs.SetInt(PlayerPrefsConstKey.Screen, (int)CurrScreen);

        bool isGame = false;
        int screen = 0;
        switch (CurrScreen)
        {
            case ScreenLevel.Low:
                screen = isGame ? 540 : 640;
                break;
            case ScreenLevel.Medium:
                screen = isGame ? 640 : 720;
                break;
            case ScreenLevel.High:
                screen = isGame ? 720 : 1080;
                break;
        }
        Screen.SetResolution((int)(screen * Screen.width / (float)Screen.height), screen, Screen.fullScreen);
        ScreenChange?.Invoke();
    }

    public void SetFrameRate(FrameRate frameRate)
    {
        CurrFrameRate = frameRate;
        switch (CurrFrameRate)
        {
            case FrameRate.Low:
                Application.targetFrameRate = 30; //60;
                break;
            case FrameRate.Medium:
                Application.targetFrameRate = 40; //60;
                break;
            case FrameRate.High:
                Application.targetFrameRate = 60; // 120;
                break;
        }
        GameEntry.PlayerPrefs.SetInt(PlayerPrefsConstKey.FrameRate, (int)CurrFrameRate);
        FrameRateChange?.Invoke();
        //#if UNITY_EDITOR
        //            Application.targetFrameRate = -1;
        //#endif
    }
}

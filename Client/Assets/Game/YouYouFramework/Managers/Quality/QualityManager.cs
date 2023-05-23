using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 画面品质设置
    /// </summary>
    public class QualityManager
    {
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


        public void SetQuality(Quality quality)
        {
            CurrQuality = quality;
            QualitySettings.SetQualityLevel(CurrQuality.ToInt(), true);
            GameEntry.Data.PlayerPrefsDataMgr.SetInt(PlayerPrefsDataMgr.EventName.QualityLevel, (int)quality);
        }
        public void SetScreen(ScreenLevel value)
        {
            CurrScreen = value;
            GameEntry.Data.PlayerPrefsDataMgr.SetInt(PlayerPrefsDataMgr.EventName.Screen, (int)CurrScreen);
            RefreshScreen();
        }

        public void RefreshScreen()
        {
            //分辨率
            bool isGame = false;
            int screen = 0;
            switch (GameEntry.Quality.CurrScreen)
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
            Screen.SetResolution((int)(screen * Screen.width / (float)Screen.height), screen, true);
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
            GameEntry.Data.PlayerPrefsDataMgr.SetInt(PlayerPrefsDataMgr.EventName.FrameRate, (int)CurrFrameRate);
//#if UNITY_EDITOR
//            Application.targetFrameRate = -1;
//#endif
        }
    }
}
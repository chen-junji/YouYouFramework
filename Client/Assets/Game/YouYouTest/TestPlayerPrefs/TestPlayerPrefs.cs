using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestPlayerPrefs : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //性能好, 存入本地存档
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.MasterVolume, 1);
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.AudioVolume, 1);
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.BGMVolume, 1);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            //获取本地存档
            GameEntry.Log(LogCategory.Normal, GameEntry.PlayerPrefs.GetFloat(PlayerPrefsConstKey.MasterVolume));
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            //性能差
            PlayerPrefs.SetFloat("TestKey", 0.5f);
        }
    }
}

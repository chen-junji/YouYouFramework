using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


public class TestAudio : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //播放背景音乐
            GameEntry.Audio.PlayBGM(BGMName.maintheme1);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //播放UI音效
            GameEntry.Audio.PlayAudio(AudioName.button_sound);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            //播放3D音效
            GameEntry.Audio.PlayAudio(AudioName.button_sound, transform.position);
        }
    }
}
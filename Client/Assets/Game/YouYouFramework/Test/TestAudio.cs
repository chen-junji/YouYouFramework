using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestAudio : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            GameEntry.Audio.PlayBGM(BGMName.maintheme1);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            GameEntry.Audio.PlayAudio(AudioName.UIClick);
        }
    }
}
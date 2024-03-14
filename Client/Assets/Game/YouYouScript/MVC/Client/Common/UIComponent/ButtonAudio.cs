using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]//脚本依赖
public class ButtonAudio : MonoBehaviour
{
    [SerializeField] private AudioName[] AudioNames = new AudioName[] { };
    private AudioName audioName;

    private Button m_Button;

    private void Start()
    {
        m_Button = GetComponent<Button>();

        if (AudioNames.Length == 0)
        {
            audioName = AudioName.button_sound;
        }
        else
        {
            audioName = AudioNames[Random.Range(0, AudioNames.Length)];
        }
        m_Button.onClick.AddListener(() =>
        {
            GameEntry.Audio.PlayAudio(audioName);
        });
    }
}
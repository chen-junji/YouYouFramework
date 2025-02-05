using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

[DisallowMultipleComponent]
[RequireComponent(typeof(Toggle))]//脚本依赖
public class ToggleAudio : MonoBehaviour
{
    [SerializeField] 
    private bool IsOffPlay;

    [SerializeField] 
    private List<AudioClip> audioClips = new List<AudioClip>();

    private Toggle m_Toggle;

    void Start()
    {
        if (audioClips.Count == 0)
        {
            GameEntry.LogError(LogCategory.Audio, "没有绑定AudioClip");
            return;
        }

        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Count)];
        if (audioClip == null)
        {
            GameEntry.LogError(LogCategory.Audio, "audioClip==null");
            return;
        }

        m_Toggle = GetComponent<Toggle>();
        m_Toggle.onValueChanged.AddListener((isOn) =>
        {
            if (IsOffPlay || isOn) GameEntry.Audio.PlayAudio(audioClip);
        });
    }
}
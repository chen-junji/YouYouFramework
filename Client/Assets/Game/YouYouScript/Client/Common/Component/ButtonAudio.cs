using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]//脚本依赖
public class ButtonAudio : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> audioClips = new List<AudioClip>();

    private Button m_Button;

    private void Start()
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

        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(() =>
        {
            GameEntry.Audio.PlayAudio(audioClip);
        });
    }
}
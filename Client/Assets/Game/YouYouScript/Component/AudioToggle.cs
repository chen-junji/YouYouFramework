using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]//脚本依赖
    public class AudioToggle : MonoBehaviour
    {
        [SerializeField] private bool IsOffPlay;

        [SerializeField] private string[] AudioId = new string[] { };
        private string id;

        private Toggle m_Toggle;

        void Start()
        {
            if (AudioId.Length == 0)
            {
                id = AudioConst.button_sound;
            }
            else
            {
                id = AudioId[Random.Range(0, AudioId.Length)];
            }

            m_Toggle = GetComponent<Toggle>();
            m_Toggle.onValueChanged.AddListener((isOn) =>
            {
                if (IsOffPlay || isOn) GameEntry.Audio.PlayAudio(id);
            });
        }
    }
}
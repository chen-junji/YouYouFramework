using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]//脚本依赖
    public class YouYouToggle : MonoBehaviour
    {
        [SerializeField] private bool IsOffPlay;

        [SerializeField] private string[] AudioId = new string[] { };
        private string id;

        private Toggle m_Toggle;
        void Start()
        {
            m_Toggle = GetComponent<Toggle>();

            if (AudioId.Length == 0)
            {
                id = CommonConst.button_sound;
            }
            else
            {
                id = AudioId[Random.Range(0, AudioId.Length)];
            }
            m_Toggle.onValueChanged.AddListener((isOn) =>
            {
                if (IsOffPlay)
                {
                    transform.DOScale(0.9f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(1.1f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(1, 0.05f).SetUpdate(true)));
                    GameEntry.Audio.PlayAudio(id);
                }
                else
                {
                    if (isOn)
                    {
                        transform.DOScale(0.9f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(1.1f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(1, 0.05f).SetUpdate(true)));
                        GameEntry.Audio.PlayAudio(id);
                    }
                }
            });
        }
    }
}
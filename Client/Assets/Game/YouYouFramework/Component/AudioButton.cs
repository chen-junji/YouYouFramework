using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]//脚本依赖
    public class AudioButton : MonoBehaviour
    {
        [SerializeField] private string[] AudioId = new string[] { };
        private string id;

        private Button m_Button;

        private void Start()
        {
            m_Button = GetComponent<Button>();

            if (AudioId.Length == 0)
            {
                id = CommonConst.button_sound;
            }
            else
            {
                id = AudioId[Random.Range(0, AudioId.Length)];
            }
            m_Button.onClick.AddListener(() =>
            {
                GameEntry.Audio.PlayAudio(id);
            });
        }
    }
}
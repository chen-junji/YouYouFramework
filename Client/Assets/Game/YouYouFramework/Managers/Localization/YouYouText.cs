using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYouFramework
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]//脚本依赖
    public class YouYouText : MonoBehaviour
    {
        private string m_Localization;
        private Text m_YouYouText;

        private void Awake()
        {
            m_YouYouText = GetComponent<Text>();
            m_Localization = m_YouYouText.text;
        }
        private void Start()
        {
            GameEntry.Event.AddEventListener(CommonEventId.ChangeLanguage, OnChangeLanguage);
            OnChangeLanguage(null);
        }
        private void OnDestroy()
        {
            GameEntry.Event.RemoveEventListener(CommonEventId.ChangeLanguage, OnChangeLanguage);
        }
        private void OnChangeLanguage(object userData)
        {
            if (!string.IsNullOrEmpty(m_Localization))
            {
                string text = GameEntry.Localization.GetString(m_Localization);
                if (!string.IsNullOrWhiteSpace(text)) m_YouYouText.text = text;
            }
        }
    }
}
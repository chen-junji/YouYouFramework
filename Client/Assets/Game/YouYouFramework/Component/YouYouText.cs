using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
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
            if (GameEntry.Localization != null && !string.IsNullOrEmpty(m_Localization))
            {
                string text = GameEntry.Localization.GetString(m_Localization);
                if (!string.IsNullOrWhiteSpace(text)) m_YouYouText.text = text;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;


[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]//脚本依赖
public class TextLocalization : MonoBehaviour
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
        GameEntry.Localization.ChangeLanguageAction += OnChangeLanguage;
        OnChangeLanguage();
    }
    private void OnDestroy()
    {
        GameEntry.Localization.ChangeLanguageAction -= OnChangeLanguage;
    }
    private void OnChangeLanguage()
    {
        if (!string.IsNullOrEmpty(m_Localization))
        {
            string text = GameEntry.Localization.GetString(m_Localization);
            if (!string.IsNullOrWhiteSpace(text)) m_YouYouText.text = text;
        }
    }
}

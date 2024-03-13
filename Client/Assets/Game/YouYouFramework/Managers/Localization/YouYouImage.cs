using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]//脚本依赖
    public class YouYouImage : MonoBehaviour
    {
        [Header("本地化语言Key")]
        [SerializeField]
        private string m_Localization;

        private Image m_Image;

        private void Awake()
        {
            m_Image = GetComponent<Image>();
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
                m_Image.AutoLoadTexture(GameEntry.Localization.GetString(m_Localization));
            }
        }
    }
}
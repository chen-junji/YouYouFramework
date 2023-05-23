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

        private void Start()
        {
            m_Image = GetComponent<Image>();

            if (GameEntry.Localization != null)
            {
                m_Image.AutoLoadSprite(GameEntry.Localization.GetString(m_Localization));
            }
        }
    }
}
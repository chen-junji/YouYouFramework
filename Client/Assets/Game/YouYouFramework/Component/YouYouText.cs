using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    /// <summary>
    /// Text×Ô¶¨Òå×ÓÀà
    /// </summary>
    public class YouYouText : Text
    {
        [Header("本地化语言Key")]
        [SerializeField]
        private string m_Localization;

        protected override void Start()
        {
            base.Start();
            if (GameEntry.Localization != null && !string.IsNullOrEmpty(m_Localization))
            {
                text = GameEntry.Localization.GetString(m_Localization);
            }
        }
    }
}
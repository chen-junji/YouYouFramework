using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    /// <summary>
    /// Text自定义子类
    /// </summary>
    public class YouYouText : Text
    {
        [Header("本地化语言的Key")]
        [SerializeField]
        private string m_Localization;

        protected override void Start()
        {
            base.Start();
            if (GameEntry.Localization != null)
            {
                text = GameEntry.Localization.GetString(m_Localization);
            }
        }
    }
}
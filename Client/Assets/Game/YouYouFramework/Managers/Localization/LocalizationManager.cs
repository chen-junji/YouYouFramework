using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 多语言枚举
    /// </summary>
    public enum YouYouLanguage
    {
        /// <summary>
        /// 中文
        /// </summary>
        Chinese = 0,
        /// <summary>
        /// 英文
        /// </summary>
        English = 1
    }


    public class LocalizationManager 
    {
        public event Action ChangeLanguageAction;

        /// <summary>
        /// 获取本地化文本内容
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            string value = null;
            if (GameEntry.DataTable.LocalizationDBModel.LocalizationDic.TryGetValue(key, out value))
            {
                return string.Format(value, args);
            }
            return value;
        }

        public void ChangeLanguage(YouYouLanguage language)
        {
            GameEntry.CurrLanguage = language;
            GameEntry.DataTable.LocalizationDBModel.LoadData();
            ChangeLanguageAction?.Invoke();
        }

        /// <summary>
        /// 根据当前系统语言, 切换游戏语言
        /// </summary>
        public void ChangeLanguageToSystem()
        {
#if !UNITY_EDITOR
            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.Chinese:
                    ChangeLanguage(YouYouLanguage.Chinese);
                    break;
                case SystemLanguage.English:
                    ChangeLanguage(YouYouLanguage.English);
                    break;
            }
#endif
        }

    }
}
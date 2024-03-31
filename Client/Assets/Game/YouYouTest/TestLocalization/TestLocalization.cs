using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

public class TestLocalization : MonoBehaviour
{
    [SerializeField] Button button;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (GameEntry.CurrLanguage == YouYouLanguage.Chinese)
            {
                GameEntry.Localization.ChangeLanguage(YouYouLanguage.English);
                GameEntry.Log(LogCategory.ZhangSan, GameEntry.Localization.GetString("测试文本"));
            }
            else if (GameEntry.CurrLanguage == YouYouLanguage.English)
            {
                GameEntry.Localization.ChangeLanguage(YouYouLanguage.Chinese);
                GameEntry.Log(LogCategory.ZhangSan, GameEntry.Localization.GetString("测试文本"));
            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
        }
    }
}

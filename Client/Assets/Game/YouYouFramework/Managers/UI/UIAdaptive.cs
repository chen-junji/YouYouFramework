using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class UIAdaptive : MonoBehaviour
{
    private float Offset = 30;
    private RectTransform rectTransform;

    private void OnDestroy()
    {
        GameEntry.Data.PlayerPrefsDataMgr.RemoveEventListener(PlayerPrefsDataMgr.EventName.Screen, OnCurrScreen);
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Offset = -Offset * 2;
        GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.Screen, OnCurrScreen);
        OnCurrScreen(null);
    }

    private void OnCurrScreen(object userData)
    {
        rectTransform.sizeDelta = new Vector2(GameEntry.UI.CurrScreen > GameEntry.UI.StandardScreen ? Offset : 0, 0);
    }
}

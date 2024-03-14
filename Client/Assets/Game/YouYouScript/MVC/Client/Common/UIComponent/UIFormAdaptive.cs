using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class UIFormAdaptive : MonoBehaviour
{
    private float Offset = 30;
    private RectTransform rectTransform;

    private void OnDestroy()
    {
        GameEntry.Model.GetModel<QualityModel>().RemoveEventListener((int)QualityModel.EventId.Screen, OnCurrScreen);
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Offset = -Offset * 2;
        GameEntry.Model.GetModel<QualityModel>().AddEventListener((int)QualityModel.EventId.Screen, OnCurrScreen);
        OnCurrScreen(null);
    }

    private void OnCurrScreen(object userData)
    {
        rectTransform.sizeDelta = new Vector2(GameEntry.UI.CurrScreen > GameEntry.UI.StandardScreen ? Offset : 0, 0);
    }
}

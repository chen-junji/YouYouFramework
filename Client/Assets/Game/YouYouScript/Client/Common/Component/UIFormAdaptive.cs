using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class UIFormAdaptive : MonoBehaviour
{
    private float Offset = 30;
    private RectTransform rectTransform;

    private void OnDestroy()
    {
        //GameEntry.Model.GetModel<QualityModel>().ScreenChange -= OnCurrScreen;
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Offset = -Offset * 2;
        //GameEntry.Model.GetModel<QualityModel>().ScreenChange += OnCurrScreen;
        OnCurrScreen();
    }

    private void OnCurrScreen()
    {
        rectTransform.sizeDelta = new Vector2(GameEntry.UI.CurrScreen > GameEntry.UI.StandardScreen ? Offset : 0, 0);
    }
}

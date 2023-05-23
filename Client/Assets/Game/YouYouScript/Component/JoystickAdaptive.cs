using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class JoystickAdaptive : MonoBehaviour
{
    private RectTransform rectTransform;
    private void OnDestroy()
    {
        //GameEntry.Event.Common.RemoveEventListener(CommonEventId.Screen, OnCurrScreen);
    }
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        //GameEntry.Event.Common.AddEventListener(CommonEventId.Screen, OnCurrScreen);
        //OnCurrScreen(null);
    }

    private void OnCurrScreen(object userData)
    {
        //rectTransform.sizeDelta = new Vector2(GameEntry.Instance.StandardWidth * (GameEntry.UI.CurrScreen - GameEntry.UI.StandardScreen), 0);
    }
}

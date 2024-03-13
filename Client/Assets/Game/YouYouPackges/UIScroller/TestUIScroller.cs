using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUIScroller : MonoBehaviour
{
    void Start()
    {
        UIScroller scroller = GetComponent<UIScroller>();
        scroller.OnItemCreate = (int index, GameObject obj) =>
        {
            obj.gameObject.name = index.ToString();
            obj.transform.GetComponentInChildren<Text>().text = index.ToString();
        };
        scroller.DataCount = 20;
        scroller.ResetScroller();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestEvent : MonoBehaviour
{
    private void OnDestroy()
    {
        //移除监听全局事件
        GameEntry.Event.RemoveEventListener(CommonEventId.TestEvent, OnTestEvent);
    }
    void Start()
    {
        //监听全局事件
        GameEntry.Event.AddEventListener(CommonEventId.TestEvent, OnTestEvent);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //触发全局事件
            GameEntry.Event.Dispatch(CommonEventId.TestEvent, 123);
        }
    }

    private void OnTestEvent(object userData)
    {
        Debug.Log(userData);
    }
}
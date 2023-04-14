using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestEvent : MonoBehaviour
{
    private void OnDestroy()
    {
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.TestEvent, OnTestEvent);
    }
    void Start()
    {
        GameEntry.Event.Common.AddEventListener(CommonEventId.TestEvent, OnTestEvent);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            GameEntry.Event.Common.Dispatch(CommonEventId.TestEvent, 123);
        }
    }

    private void OnTestEvent(object userData)
    {
        Debug.Log(userData);
    }
}
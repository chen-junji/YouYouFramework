using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestEvent : MonoBehaviour
{
    private void OnDestroy()
    {
        GameEntry.Event.RemoveEventListener(EventName.TestEvent, OnTestEvent);
    }
    void Start()
    {
        GameEntry.Event.AddEventListener(EventName.TestEvent, OnTestEvent);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            GameEntry.Event.Dispatch(EventName.TestEvent, 123);
        }
    }

    private void OnTestEvent(object userData)
    {
        Debug.Log(userData);
    }
}
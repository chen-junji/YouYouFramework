using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestEvent : MonoBehaviour
{
    void Start()
    {
        GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.RegComplete, OnRegComplete);
    }

    private void OnRegComplete(object userData)
    {
        Debug.Log(userData);
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.A))
        //{
        //    GameEntry.Event.CommonEvent.Dispatch(CommonEventId.RegComplete, 123);
        //}
    }

    private void OnDestroy()
    {
        GameEntry.Event.CommonEvent.RemoveEventListener(CommonEventId.RegComplete, OnRegComplete);
    }
}
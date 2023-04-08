using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestEvent : MonoBehaviour
{
    void Start()
    {
        //GameEntry.Event.Common.AddEventListener(CommonEventId.RegComplete, OnRegComplete);
    }

    private void OnRegComplete(object userData)
    {
        Debug.Log(userData);
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.A))
        //{
        //    GameEntry.Event.Common.Dispatch(CommonEventId.RegComplete, 123);
        //}
    }

    private void OnDestroy()
    {
        //GameEntry.Event.Common.RemoveEventListener(CommonEventId.RegComplete, OnRegComplete);
    }
}
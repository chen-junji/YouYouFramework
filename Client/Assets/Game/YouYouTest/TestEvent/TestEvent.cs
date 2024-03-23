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

        //移除监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<TestModel>().RemoveEventListener((int)TestModel.TestEvent.TestEvent1, OnTestEvent);
    }
    void Start()
    {
        //监听全局事件
        GameEntry.Event.AddEventListener(CommonEventId.TestEvent, OnTestEvent);

        //监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<TestModel>().AddEventListener((int)TestModel.TestEvent.TestEvent1, OnTestEvent);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //触发全局事件
            GameEntry.Event.Dispatch(CommonEventId.TestEvent, 123);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //触发某个Model内的某个数据刷新的事件
            GameEntry.Model.GetModel<TestModel>().Dispatch((int)TestModel.TestEvent.TestEvent1);
        }
    }

    private void OnTestEvent(object userData)
    {
        Debug.Log(userData);
    }
}
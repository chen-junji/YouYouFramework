using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class MainCtrl
{
    public static MainCtrl Instance { get; private set; } = new();

    //假装这是后端消息
    public Action<int> onTest;

    public MainCtrl()
    {
        //监听后端消息
        onTest += OnTest;
    }

    public void SendTest()
    {
        //假装请求后端
        onTest?.Invoke(1);
    }

    private void OnTest(int value)
    {
        if (value == -1)
        {
            //错误码
            return;
        }
        else if (value == -2)
        {
            //错误码
            return;
        }
        //假装收到后端消息, 把数据给到Model
        GameEntry.Model.GetModel<MainModel>().OnTest(value);
    }
}

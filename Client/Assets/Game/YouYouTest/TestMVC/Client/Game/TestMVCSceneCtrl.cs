using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestMVCSceneCtrl : SingletonMono<TestMVCSceneCtrl>
{
    [SerializeField]
    private RoleCtrl roleCtrl;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //移除监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<GameModel>().TestEvent -= OnTestEvent;
    }
    void Start()
    {
        //监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<GameModel>().TestEvent += OnTestEvent;

        GameEntry.Input.SetEnable(true);

        DialogForm.ShowForm("框架内部流程全部加载完毕, 已经进入登录流程", "登录流程");
    }
    private void Update()
    {
        CameraFollowCtrl.Instance.transform.position = roleCtrl.transform.position;

        if (Input.GetKeyUp(KeyCode.S))
        {
            //触发某个Model内的某个数据刷新的事件
            GameEntry.Model.GetModel<GameModel>().DispatchTestEvent();
        }
    }

    private void OnTestEvent()
    {

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestMVCSceneCtrl : SingletonMono<TestMVCSceneCtrl>
{
    [SerializeField]
    private RoleCtrl roleCtrl;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        //移除监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<TestModel>().RemoveEventListener((int)TestModel.TestEvent.TestEvent1, OnTestEvent1);
    }
    void Start()
    {
        //监听某个Model内的某个数据刷新的事件
        GameEntry.Model.GetModel<TestModel>().AddEventListener((int)TestModel.TestEvent.TestEvent1, OnTestEvent1);

        GameForm.ShowForm();
        GameEntry.Input.SetEnable(true);

        DialogForm.ShowForm("框架内部流程全部加载完毕, 已经进入登录流程", "登录流程");
    }
    private void Update()
    {
        CameraFollowCtrl.Instance.transform.position = roleCtrl.transform.position;

        if (Input.GetKeyUp(KeyCode.S))
        {
            //触发某个Model内的某个数据刷新的事件
            GameEntry.Model.GetModel<TestModel>().Dispatch((int)TestModel.TestEvent.TestEvent1);
        }
    }

    private void OnTestEvent1(object userData)
    {

    }
}

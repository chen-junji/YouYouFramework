using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestFsm : MonoBehaviour
{
    private TestFsmMgr fsmMgr;
    private void OnDestroy()
    {
        //销毁状态机
        fsmMgr.CurrFsm.ShutDown();
    }
    void Start()
    {
        //初始化状态机
        fsmMgr = new TestFsmMgr();
        fsmMgr.Init();
    }
    void Update()
    {
        fsmMgr.OnUpdate();

        if (Input.GetKeyUp(KeyCode.A))
        {
            //切换状态
            fsmMgr.ChangeState(TestFsmMgr.TestFsmState.State1);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //切换状态
            fsmMgr.ChangeState(TestFsmMgr.TestFsmState.State2);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestGamePlayFsm : MonoBehaviour
{
    private TestGamePlayFsmMgr fsmMgr;
    private void OnDestroy()
    {
        //销毁状态机
        fsmMgr.CurrFsm.Destroy();
    }
    void Start()
    {
        //初始化状态机
        fsmMgr = new TestGamePlayFsmMgr();
        fsmMgr.Init();
    }
    void Update()
    {
        fsmMgr.OnUpdate();

        if (Input.GetKeyUp(KeyCode.A))
        {
            //切换状态
            fsmMgr.CurrFsm.SetParam(TestGamePlayFsmMgr.ParamConst.IntState2, 0);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //切换状态
            fsmMgr.CurrFsm.SetParam(TestGamePlayFsmMgr.ParamConst.IntState2, 1);
        }
    }
}
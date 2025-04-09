using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestFsmMgr
{
    public enum EState
    {
        State1,
        State2,
    }
    /// <summary>
    /// 当前状态机
    /// </summary>
    public Fsm<TestFsmMgr> CurrFsm { get; private set; }

    internal void Init()
    {
        //得到枚举的长度
        int count = Enum.GetNames(typeof(EState)).Length;
        FsmState<TestFsmMgr>[] states = new FsmState<TestFsmMgr>[count];

        states[(byte)EState.State1] = new TestFsmState1();
        states[(byte)EState.State2] = new TestFsmState2();

        //创建流程的状态机
        CurrFsm = GameEntry.Fsm.Create(this, states);
    }
    internal void OnUpdate()
    {
        CurrFsm.OnUpdate();
    }
    public void ChangeState(EState state)
    {
        CurrFsm.ChangeState((sbyte)state);
    }
}

public class TestFsmState1 : FsmState<TestFsmMgr>
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnEnter()");
    }
    public override void OnLeave(int newState)
    {
        base.OnLeave(newState);
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnLeave()");
    }
    public override void OnUpdate(float elapseSeconds)
    {
        base.OnUpdate(elapseSeconds);
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>OnUpdate()");
    }
    internal override void OnDestroy()
    {
        base.OnDestroy();
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>OnDestroy()");
    }

}
public class TestFsmState2 : FsmState<TestFsmMgr>
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnEnter()");
    }
    public override void OnLeave(int newState)
    {
        base.OnLeave(newState);
        GameEntry.Log(LogCategory.Normal, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnLeave()");
    }
}
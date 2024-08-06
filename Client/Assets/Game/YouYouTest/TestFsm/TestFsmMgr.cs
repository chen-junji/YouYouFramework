using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// 某个状态机的管理器
/// </summary>
public class TestFsmMgr
{
    public enum EState
    {
        State1,
        State2,
    }
    public class ParamConst
    {
        public const string IntState2 = "IntState2";
    }

    /// <summary>
    /// 当前状态机
    /// </summary>
    public Fsm<TestFsmMgr> CurrFsm { get; private set; }

    /// <summary>
    /// 当前状态Type
    /// </summary>
    public EState CurrProcedureState
    {
        get
        {
            return (EState)CurrFsm.CurrStateType;
        }
    }

    internal void Init()
    {
        //得到枚举的长度
        int count = Enum.GetNames(typeof(EState)).Length;
        FsmState<TestFsmMgr>[] states = new FsmState<TestFsmMgr>[count];

        //这个状态机跟Unity的动画状态机的原理是一模一样的
        //如果当前状态的其中一条过渡线的全部条件满足, 就会切换到这条过渡线对应的状态
        states[(byte)EState.State1] = new TestFsmState1()
        {
            Transitions = new()
            {
                //这里定义状态机的过渡线
                new()
                {
                    TargetState = (int)EState.State2,
                    //这里定义状态机的过渡条件(Int String Bool之类的都可以, 反正是func委托)
                    FsmConditions = new()
                    {
                        new(() =>
                        {
                            return CurrFsm.GetParam<int>(ParamConst.IntState2) == 1;
                        }),
                    },
                    //这里也是状态机的过渡条件(Trigger)
                    FsmConditionTriggers = new()
                    {
                    }
                },
            }
        };
        states[(byte)EState.State2] = new TestFsmState2()
        {
            Transitions = new()
            {
                new()
                {
                    TargetState = (int)EState.State1,
                    FsmConditions = new()
                    {
                        new(() =>
                        {
                            return CurrFsm.GetParam<int>(ParamConst.IntState2) == 0;
                        }),
                    },
                    FsmConditionTriggers = new()
                    {
                    }
                },
            }
        };

        CurrFsm = GameEntry.Fsm.Create(this, states);
    }
    internal void OnUpdate()
    {
        CurrFsm.OnUpdate();
    }

    public void SetData<TData>(string key, TData value)
    {
        CurrFsm.SetParam(key, value);
    }
    public TData GetDada<TData>(string key)
    {
        return CurrFsm.GetParam<TData>(key);
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
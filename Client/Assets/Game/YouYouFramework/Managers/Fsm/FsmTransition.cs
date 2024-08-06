using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态过渡线, 相当于Animator动画状态机的连线
/// </summary>
public class FsmTransition
{
    /// <summary>
    /// 目标状态
    /// </summary>
    public sbyte TargetState;

    /// <summary>
    /// 状态切换的条件列表
    /// </summary>
    public List<FsmCondition> FsmConditions = new();
    public List<string> FsmConditionTriggers = new();

    //某个状态过渡线 当前是否能切到目标状态?
    public bool CanTransitions()
    {
        //过渡条件列表, 需要所有条件全部满足, 才能过渡状态
        bool retValue = true;
        foreach (var fsmCondition in FsmConditions)
        {
            if (fsmCondition.FuncCondition() == false)
            {
                retValue = false;
            }
        }
        return retValue;
    }

}

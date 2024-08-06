using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FsmCondition
{
    public Func<bool> FuncCondition { get; private set; }

    public FsmCondition(Func<bool> func)
    {
        FuncCondition = func;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    public class GPFsmCondition
    {
        public Func<bool> FuncCondition { get; private set; }

        public GPFsmCondition(Func<bool> func)
        {
            FuncCondition = func;
        }
    }
}
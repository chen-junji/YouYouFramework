using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 状态机基类
    /// </summary>
    public abstract class GPFsmBase
    {
        /// <summary>
        /// 当前状态的类型
        /// </summary>
        public sbyte CurrStateType;

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public abstract void Destroy();

    }
}
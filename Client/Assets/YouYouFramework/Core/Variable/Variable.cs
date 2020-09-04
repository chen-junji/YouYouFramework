//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 变量泛型基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Variable<T> : VariableBase
    {
        /// <summary>
        /// 当前存储的真实值
        /// </summary>
        public T Value;

        /// <summary>
        /// 变量类型
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }
    }
}
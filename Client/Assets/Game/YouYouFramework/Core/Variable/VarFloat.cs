
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    /// <summary>
    /// float变量
    /// </summary>
    public class VarFloat : Variable<float>
    {
        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public static VarFloat Alloc()
        {
            VarFloat var = GameEntry.Pool.VarObjectPool.DequeueVarObject<VarFloat>();
            var.Value = 0;
            var.Retain();
            return var;
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static VarFloat Alloc(float value)
        {
            VarFloat var = Alloc();
            var.Value = value;
            return var;
        }

        /// <summary>
        /// VarFloat -> float
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator float(VarFloat value)
        {
            return value.Value;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// bool变量
    /// </summary>
    public class VarBool : Variable<bool>
    {
        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public static VarBool Alloc()
        {
            VarBool var = GameEntry.Pool.VarObjectPool.DequeueVarObject<VarBool>();
            var.Value = false;
            var.Retain();
            return var;
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static VarBool Alloc(bool value)
        {
            VarBool var = Alloc();
            var.Value = value;
            return var;
        }

        /// <summary>
        /// VarBool -> bool
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator bool(VarBool value)
        {
            return value.Value;
        }
    }
}
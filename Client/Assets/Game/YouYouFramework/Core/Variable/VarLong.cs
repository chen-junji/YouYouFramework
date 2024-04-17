using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    /// <summary>
    /// long变量
    /// </summary>
    public class VarLong : Variable<long>
    {
        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public static VarLong Alloc()
        {
            VarLong var = GameEntry.Pool.VarObjectPool.DequeueVarObject<VarLong>();
            var.Value = 0;
            var.Retain();
            return var;
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static VarLong Alloc(long value)
        {
            VarLong var = Alloc();
            var.Value = value;
            return var;
        }

        /// <summary>
        /// VarLong -> long
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator long(VarLong value)
        {
            return value.Value;
        }
    }
}
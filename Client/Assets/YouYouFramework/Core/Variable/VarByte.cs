//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// byte变量
    /// </summary>
    public class VarByte : Variable<byte>
    {
        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public static VarByte Alloc()
        {
            VarByte var = GameEntry.Pool.DequeueVarObject<VarByte>();
            var.Value = 0;
            var.Retain();
            return var;
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static VarByte Alloc(byte value)
        {
            VarByte var = Alloc();
            var.Value = value;
            return var;
        }

        /// <summary>
        /// VarByte -> byte
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator byte(VarByte value)
        {
            return value.Value;
        }
    }
}
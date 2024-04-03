using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    public class VarVector2 : Variable<Vector2>
    {
        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public static VarVector2 Alloc()
        {
            VarVector2 var = GameEntry.Pool.VarObjectPool.DequeueVarObject<VarVector2>();
            var.Value = Vector2.zero; ;
            var.Retain();
            return var;
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static VarVector2 Alloc(VarVector2 value)
        {
            VarVector2 var = Alloc();
            var.Value = value;
            return var;
        }

        /// <summary>
        /// VarString -> string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Vector2(VarVector2 value)
        {
            return value.Value;
        }
    }
}
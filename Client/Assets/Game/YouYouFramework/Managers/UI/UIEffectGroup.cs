using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// UI特效分组
    /// </summary>
    [Serializable]
    public class UIEffectGroup
    {
        /// <summary>
        /// 排序
        /// </summary>
        public ushort Order;
        /// <summary>
        /// 特效组
        /// </summary>
        public List<Transform> Group = new List<Transform>();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// UI分组
    /// </summary>
    [System.Serializable]
    public class UIGroup
    {
        /// <summary>
        /// 分组编号
        /// </summary>
        public byte Id;

        /// <summary>
        /// 基础排序
        /// </summary>
        public ushort BaseOrder;

        /// <summary>
        /// 
        /// </summary>
        public Transform Group;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// UI层级管理
    /// </summary>
    public class UILayer
    {
        private Dictionary<byte, ushort> m_UILayerDic;

        public UILayer()
        {
            m_UILayerDic = new Dictionary<byte, ushort>();
        }

        /// <summary>
        /// 初始化基础排序
        /// </summary>
        /// <param name="groups"></param>
        internal void Init(UIGroup[] groups)
        {
            int len = groups.Length;
            for (int i = 0; i < len; i++)
            {
                UIGroup group = groups[i];
                m_UILayerDic[group.Id] = group.BaseOrder;
            }
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        /// <param name="formBase">窗口</param>
        /// <param name="isAdd">true:增加  false:减少</param>
        internal void SetSortingOrder(UIFormBase formBase, bool isAdd)
        {
            if (!m_UILayerDic.ContainsKey(formBase.GroupId)) return;

            if (isAdd)
            {
                m_UILayerDic[formBase.GroupId] += 10;
            }
            else
            {
                if (formBase.CurrCanvas.sortingOrder == m_UILayerDic[formBase.GroupId])
                {
                    m_UILayerDic[formBase.GroupId] -= 10;
                }
            }

            formBase.CurrCanvas.sortingOrder = m_UILayerDic[formBase.GroupId];
        }
    }
}
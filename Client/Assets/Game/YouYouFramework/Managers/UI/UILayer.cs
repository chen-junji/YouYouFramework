using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
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

            //初始化基础排序
            for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
            {
                UIGroup group = GameEntry.Instance.UIGroups[i];
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
            if (formBase.SysUIForm.DisableUILayer == 1)
            {
                return;
            }
            if (m_UILayerDic.ContainsKey(formBase.SysUIForm.UIGroupId) == false)
            {
                return;
            }

            if (isAdd)
            {
                m_UILayerDic[formBase.SysUIForm.UIGroupId] += 10;
            }
            else
            {
                m_UILayerDic[formBase.SysUIForm.UIGroupId] -= 10;
            }
        }

        internal int GetCurrSortingOrder(UIFormBase formBase)
        {
            return m_UILayerDic[formBase.SysUIForm.UIGroupId];
        }
    }
}
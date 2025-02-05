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
        /// <param name="Sys_UIFormEntity">窗口</param>
        /// <param name="isAdd">true:增加  false:减少</param>
        internal void SetSortingOrder(Sys_UIFormEntity sys_UIForm, bool isAdd)
        {
            if (sys_UIForm.DisableUILayer == 1)
            {
                return;
            }
            if (m_UILayerDic.ContainsKey(sys_UIForm.UIGroupId) == false)
            {
                return;
            }

            if (isAdd)
            {
                m_UILayerDic[sys_UIForm.UIGroupId] += 10;
            }
            else
            {
                m_UILayerDic[sys_UIForm.UIGroupId] -= 10;
            }
        }

        internal int GetCurrSortingOrder(Sys_UIFormEntity sys_UIForm)
        {
            if (sys_UIForm.DisableUILayer == 1)
            {
                return 0;
            }
            return m_UILayerDic[sys_UIForm.UIGroupId];
        }
    }
}
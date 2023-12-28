using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class UIPool
    {
        /// <summary>
        /// 对象池中的列表
        /// </summary>
        private LinkedList<UIFormBase> m_UIFormList;

        /// <summary>
        /// UI回池后过期时间_秒
        /// </summary>
        public float UIExpire { get; private set; }
        /// <summary>
        /// UI释放间隔_秒
        /// </summary>
        public float ClearInterval { get; private set; }
        /// <summary>
        /// 下次运行时间
        /// </summary>
        private float m_NextRunTime = 0f;


        public UIPool()
        {
            m_UIFormList = new LinkedList<UIFormBase>();

            UIExpire = Main.MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_Expire, Main.MainEntry.CurrDeviceGrade);
            ClearInterval = Main.MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_ClearInterval, Main.MainEntry.CurrDeviceGrade);
        }
        internal void OnUpdate()
        {
            if (Time.time > m_NextRunTime + ClearInterval)
            {
                m_NextRunTime = Time.time;

                //释放UI对象池
                CheckClear();
            }
        }

        /// <summary>
        /// 对象获取
        /// </summary>
        internal UIFormBase Dequeue(int uiFormId)
        {
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    GameEntry.UI.ShowUI(curr.Value);
                    m_UIFormList.Remove(curr.Value);
                    return curr.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        internal void EnQueue(UIFormBase form)
        {
            m_UIFormList.AddLast(form);
        }

        /// <summary>
        /// 检查对象池释放
        /// </summary>
        internal void CheckClear()
        {
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
            {
                if (curr.Value.SysUIForm.IsLock != 1 && Time.time > curr.Value.CloseTime + UIExpire)
                {
                    LinkedListNode<UIFormBase> next = curr.Next;

                    //GameEntry.Log(LogCategory.Loader, "释放==" + curr.Value.gameObject);
                    Release(curr.Value);

                    curr = next;
                }
                else
                {
                    curr = curr.Next;
                }
            }
        }

        internal void Release(string uiFormName)
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetEntity(uiFormName).Id;
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    Release(curr.Value);
                    break;
                }
            }
        }
        public void Release(UIFormBase uIBase)
        {
            m_UIFormList.Remove(uIBase);
            Object.Destroy(uIBase.gameObject);
        }

        /// <summary>
        /// 立即强制清除全部窗口界面
        /// </summary>
        internal void ReleaseAll()
        {
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
            {
                LinkedListNode<UIFormBase> next = curr.Next;

                GameEntry.Log(LogCategory.Loader, "释放==" + curr.Value.gameObject);
                Release(curr.Value);
                curr = next;
            }
        }

        public UIFormBase GetUIForm(int uiFormId)
        {
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    return curr.Value;
                }
            }
            return null;
        }
    }
}
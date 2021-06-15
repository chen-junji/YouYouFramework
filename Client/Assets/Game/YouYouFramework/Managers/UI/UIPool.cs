using DG.Tweening;
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

        public UIPool()
        {
            m_UIFormList = new LinkedList<UIFormBase>();
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
            GameEntry.UI.HideUI(form);
            m_UIFormList.AddLast(form);
        }

        /// <summary>
        /// 检查对象池释放
        /// </summary>
        internal void CheckClear()
        {
            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
            {
                if (curr.Value.SysUIForm.IsLock != 1 && Time.time > curr.Value.CloseTime + GameEntry.UI.UIExpire)
                {
                    Object.Destroy(curr.Value.gameObject);
                    GameEntry.Pool.ReleaseInstanceResource(curr.Value.gameObject.GetInstanceID());

                    LinkedListNode<UIFormBase> next = curr.Next;
                    m_UIFormList.Remove(curr.Value);
                    curr = next;
                }
                else
                {
                    curr = curr.Next;
                }
            }
        }
    }
}
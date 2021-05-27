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
		/// UI´°¿Ú³ö³Ø
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		internal UIFormBase Dequeue(int uiFormId)
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.SysUIForm.Id == uiFormId)
				{
					m_UIFormList.Remove(curr.Value);
					//Debug.LogError("³ö³Ø=="  + curr.Value.gameObject.name);
					return curr.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// UI´°¿ÚÈë³Ø
		/// </summary>
		/// <param name="form"></param>
		internal void EnQueue(UIFormBase form)
		{
			GameEntry.UI.HideUI(form);
			m_UIFormList.AddLast(form);
			//Debug.LogError("Èë³Ø==" + form.gameObject.name);
		}

		/// <summary>
		/// ¼ì²éÊÇ·ñ¿ÉÒÔÊÍ·Å
		/// </summary>
		internal void CheckClear()
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
			{
				if (curr.Value.SysUIForm.IsLock != 1 && Time.time > curr.Value.CloseTime + GameEntry.UI.UIExpire)
				{
					//Ïú»ÙUI
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

        /// <summary>
        /// 打开UI时候检查队列
        /// </summary>
        internal void CheckByOpenUI()
        {
            if (m_UIFormList.Count <= GameEntry.UI.UIPoolMaxCount) return;

            for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
            {
                if (m_UIFormList.Count == GameEntry.UI.UIPoolMaxCount + 1) break;

				if (curr.Value.SysUIForm.IsLock != 1)
				{
					LinkedListNode<UIFormBase> next = curr.Next;
					m_UIFormList.Remove(curr.Value);

					//Ïú»ÙUI
					Object.Destroy(curr.Value.gameObject);
					GameEntry.Pool.ReleaseInstanceResource(curr.Value.gameObject.GetInstanceID());

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// UI窗口对象池
	/// </summary>
	public class UIPool
	{
		/// <summary>
		/// 对象池中的链表
		/// </summary>
		private LinkedList<UIFormBase> m_UIFormList;

		public UIPool()
		{
			m_UIFormList = new LinkedList<UIFormBase>();
		}

		/// <summary>
		/// 从池中获取UI窗口
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		internal UIFormBase Dequeue(int uiFormId)
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					m_UIFormList.Remove(curr.Value);
					return curr.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// UI窗口回池
		/// </summary>
		/// <param name="form"></param>
		internal void EnQueue(UIFormBase form)
		{
			GameEntry.UI.HideUI(form);
			m_UIFormList.AddLast(form);
		}

		/// <summary>
		/// 检查是否可以释放
		/// </summary>
		internal void CheckClear()
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
			{
				if (!curr.Value.IsLock && Time.time > curr.Value.CloseTime + GameEntry.UI.UIExpire)
				{
					//销毁UI
					Object.Destroy(curr.Value.gameObject);
					GameEntry.Pool.ReleaseInstanceResource(curr.Value.GetInstanceID());

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
		/// 根据UI窗口数量,检查是否可以释放
		/// </summary>
		internal void CheckByOpenUI()
		{
			if (m_UIFormList.Count <= GameEntry.UI.UIPoolMaxCount) return;

			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
			{
				if (m_UIFormList.Count == GameEntry.UI.UIPoolMaxCount + 1) break;

				if (!curr.Value.IsLock)
				{
					LinkedListNode<UIFormBase> next = curr.Next;
					m_UIFormList.Remove(curr.Value);

					//销毁UI
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
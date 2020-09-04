using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	public class TimeManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// 定时器链表
		/// </summary>
		private LinkedList<TimeAction> m_TimeActionList;

		internal TimeManager()
		{
			m_TimeActionList = new LinkedList<TimeAction>();

		}
		public void Dispose()
		{
			m_TimeActionList.Clear();
		}
		public override void Init()
		{
		}
		internal void OnUpdate()
		{
			for (LinkedListNode<TimeAction> curr = m_TimeActionList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.OnStarAction.Target == null || curr.Value.OnStarAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				if (curr.Value.OnUpdateAction.Target == null || curr.Value.OnUpdateAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				if (curr.Value.OnCompleteAction.Target == null || curr.Value.OnCompleteAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				curr.Value.OnUpdate();
			}
		}


		/// <summary>
		/// 注册定时器
		/// </summary>
		/// <param name="action"></param>
		internal void RegisterTimeAction(TimeAction action)
		{
			m_TimeActionList.AddLast(action);
		}
		/// <summary>
		/// 移除定时器
		/// </summary>
		/// <param name="action"></param>
		internal void RemoveTimeAction(TimeAction action)
		{
			m_TimeActionList.Remove(action);
			GameEntry.Pool.EnqueueClassObject(action);
		}
		/// <summary>
		/// 根据定时器名字 删除定时器
		/// </summary>
		/// <param name="timeName"></param>
		public void RemoveTimeActionByName(string timeName)
		{
			LinkedListNode<TimeAction> curr = m_TimeActionList.First;
			while (curr != null)
			{
				if (curr.Value.TimeName.Equals(timeName, StringComparison.CurrentCultureIgnoreCase))
				{
					RemoveTimeAction(curr.Value);
					break;
				}
				curr = curr.Next;
			}
		}

		/// <summary>
		/// 创建定时器
		/// </summary>
		/// <returns></returns>
		public TimeAction CreateTimeAction()
		{
			return GameEntry.Pool.DequeueClassObject<TimeAction>();
		}

		/// <summary>
		/// 延迟一帧
		/// </summary>
		/// <param name="onComplete"></param>
		public void Yield(BaseAction onComplete)
		{
			GameEntry.Instance.StartCoroutine(YieldCoroutine(onComplete));
		}
		private IEnumerator YieldCoroutine(BaseAction onComplete)
		{
			yield return null;
			if (onComplete != null) onComplete();
		}

		/// <summary>
		/// 修改时间缩放
		/// </summary>
		/// <param name="toTimeScale">缩放的值</param>
		/// <param name="continueTime">持续时间</param>
		public void ChangeTimeScale(float toTimeScale, float continueTime)
		{
			Time.timeScale = toTimeScale;
			GameEntry.Time.CreateTimeAction().Init(null, continueTime, 0, 0, () =>
			 {
				 Time.timeScale = 1;
			 }).Run();
		}



	}
}
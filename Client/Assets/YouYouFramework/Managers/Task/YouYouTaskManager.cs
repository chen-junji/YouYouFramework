using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class YouYouTaskManager : ManagerBase
{
	private List<YouYouTask> TaskList;

	internal override void Init()
	{
		TaskList = new List<YouYouTask>();
	}
	public void Add(Action<Action> task)
	{
		if (task == null) return;

		GameEntry.UI.OpenUIForm(UIFormId.UICircle);

		YouYouTask youTask = new YouYouTask(task);
		youTask.OnComplete = () =>
		{
			if (youTask.isComplete) return;
			youTask.isComplete = true;

			TaskList.Remove(youTask);
			if (TaskList.Count == 0) GameEntry.UI.CloseUIForm(UIFormId.UICircle);
		};
		TaskList.Add(youTask);
		youTask.Task?.Invoke(youTask.OnComplete);
	}

	public void OnUpdate()
	{
#if UNITY_EDITOR
		if (Input.GetKeyUp(KeyCode.T))
		{
			Debug.LogError("TaskList.Count==" + TaskList.Count);
			for (int i = 0; i < TaskList.Count; i++)
			{
				Debug.LogError(TaskList[i].Task.Method);
			}
		}
	}
#endif
}

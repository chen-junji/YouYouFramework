using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouYouTask
{
	public Action<Action> Task { get; private set; }
	public bool isComplete = false;
	public Action OnComplete;


	public YouYouTask(Action<Action> task)
	{
		Task = task;
	}


}

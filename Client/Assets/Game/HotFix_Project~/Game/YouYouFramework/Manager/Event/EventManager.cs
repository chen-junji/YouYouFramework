using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Hotfix
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public class EventManager 
	{
		public CommonEvent CommonEvent { get; private set; }

		internal EventManager()
		{
			CommonEvent = new CommonEvent();
		}
	}
}

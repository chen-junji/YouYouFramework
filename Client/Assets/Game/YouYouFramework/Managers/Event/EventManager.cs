using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public class EventManager 
	{
		/// <summary>
		/// 通用事件
		/// </summary>
		public CommonEvent Common { get; private set; }

		internal EventManager()
		{
			Common = new CommonEvent();
		}
	}
}

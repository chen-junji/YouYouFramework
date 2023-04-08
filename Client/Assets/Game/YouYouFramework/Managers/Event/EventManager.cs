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
		/// Socket事件
		/// </summary>
		public SocketEvent SocketEvent { get; private set; }
		public WebSocketEvent WebSocketEvent { get; private set; }
		/// <summary>
		/// 通用事件
		/// </summary>
		public CommonEvent Common { get; private set; }

		internal EventManager()
		{
			SocketEvent = new SocketEvent();
			WebSocketEvent = new WebSocketEvent();
			Common = new CommonEvent();
		}
	}
}

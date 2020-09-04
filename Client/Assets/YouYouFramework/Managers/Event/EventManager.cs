using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public class EventManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// Socket事件
		/// </summary>
		public SocketEvent SocketEvent { get; private set; }
		public WebSocketEvent WebSocketEvent { get; private set; }
		/// <summary>
		/// 通用事件
		/// </summary>
		public CommonEvent CommonEvent { get; private set; }

		public EventManager()
		{
			SocketEvent = new SocketEvent();
			WebSocketEvent = new WebSocketEvent();
			CommonEvent = new CommonEvent();
		}

		public void Dispose()
		{
			SocketEvent.Dispose();
			CommonEvent.Dispose();
			WebSocketEvent.Dispose();
		}

		public override void Init()
		{

		}
	}
}

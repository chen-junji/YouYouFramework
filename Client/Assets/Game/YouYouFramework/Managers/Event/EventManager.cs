using Main;
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

        public void Dispatch(EventName key)
        {
            Common.Dispatch(key.ToString());
        }
        public void Dispatch(EventName key, object userData)
        {
            Common.Dispatch(key.ToString(), userData);
        }

        public void AddEventListener(EventName key, CommonEvent.OnActionHandler handler)
        {
            Common.AddEventListener(key.ToString(), handler);
        }
        public void RemoveEventListener(EventName key, CommonEvent.OnActionHandler handler)
        {
            Common.RemoveEventListener(key.ToString(), handler);
        }
        public void RemoveEventListenerAll(EventName key)
        {
            Common.RemoveEventListenerAll(key.ToString());
        }
    }
}

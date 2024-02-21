using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
    public enum CommonEventId
    {
        TestEvent,
        GameEntryOnUpdate,
        GameEntryOnApplicationQuit,
        GameEntryOnApplicationPause,
        LoadingSceneUpdate,
        MasterVolume,
        BGMVolume,
        AudioVolume,
    }

    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager 
	{
        /// <summary>
        /// 通用事件
        /// </summary>
        private CommonEvent Common;

		internal EventManager()
		{
			Common = new CommonEvent();
		}

        public void Dispatch(CommonEventId key)
        {
            Common.Dispatch((int)key);
        }
        public void Dispatch(CommonEventId key, object userData)
        {
            Common.Dispatch((int)key, userData);
        }

        public void AddEventListener(CommonEventId key, CommonEvent.OnActionHandler handler)
        {
            Common.AddEventListener((int)key, handler);
        }
        public void RemoveEventListener(CommonEventId key, CommonEvent.OnActionHandler handler)
        {
            Common.RemoveEventListener((int)key, handler);
        }
        public void RemoveEventListenerAll(CommonEventId key)
        {
            Common.RemoveEventListenerAll((int)key);
        }
    }
}

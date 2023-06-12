using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class DataMgrBase<T> where T : Enum
    {
        private CommonEvent CommonEvent;
        public DataMgrBase()
        {
            CommonEvent = new CommonEvent();
        }

        public void Dispatch(T key)
        {
            CommonEvent.Dispatch(key.ToString());
        }
        public void Dispatch(T key, object userData)
        {
            CommonEvent.Dispatch(key.ToString(), userData);
        }

        public void AddEventListener(T key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.AddEventListener(key.ToString(), handler);
        }
        public void RemoveEventListener(T key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.RemoveEventListener(key.ToString(), handler);
        }
        public void RemoveEventListenerAll(T key)
        {
            CommonEvent.RemoveEventListenerAll(key.ToString());
        }
    }
}
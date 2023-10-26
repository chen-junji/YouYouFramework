using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

namespace Main
{
    public class Observable<T, K> : Singleton<T>
        where T : new()
        where K : Enum
    {
        private CommonEvent CommonEvent;
        public Observable()
        {
            CommonEvent = new CommonEvent();
        }

        public void Dispatch(K key)
        {
            CommonEvent.Dispatch(key.ToString());
        }
        public void Dispatch(K key, object userData)
        {
            CommonEvent.Dispatch(key.ToString(), userData);
        }

        public void AddEventListener(K key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.AddEventListener(key.ToString(), handler);
        }
        public void RemoveEventListener(K key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.RemoveEventListener(key.ToString(), handler);
        }
        public void RemoveEventListenerAll(K key)
        {
            CommonEvent.RemoveEventListenerAll(key.ToString());
        }
    }
}
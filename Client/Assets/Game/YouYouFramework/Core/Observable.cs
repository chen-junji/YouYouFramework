using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

namespace Main
{
    public class Observable
    {
        private CommonEvent CommonEvent;
        public Observable()
        {
            CommonEvent = new CommonEvent();
        }

        public void Dispatch(int key)
        {
            CommonEvent.Dispatch(key);
        }
        public void Dispatch(int key, object userData)
        {
            CommonEvent.Dispatch(key, userData);
        }

        public void AddEventListener(int key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.AddEventListener(key, handler);
        }
        public void RemoveEventListener(int key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.RemoveEventListener(key, handler);
        }
        public void RemoveEventListenerAll(int key)
        {
            CommonEvent.RemoveEventListenerAll(key);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
{
    /// <summary>
    /// 通用事件
    /// </summary>
    public class CommonEvent
    {
        public delegate void OnActionHandler(object userData);
        private Dictionary<int, LinkedList<OnActionHandler>> dic = new Dictionary<int, LinkedList<OnActionHandler>>();

        #region AddEventListener 观察者监听事件
        /// <summary>
        /// 观察者监听事件
        /// </summary>
        public void AddEventListener(int key, OnActionHandler handler)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);
            if (lstHandler == null)
            {
                lstHandler = new LinkedList<OnActionHandler>();
                dic[key] = lstHandler;
            }
            lstHandler.AddLast(handler);
        }
        #endregion

        #region RemoveEventListener 观察者移除监听事件
        /// <summary>
        /// 观察者移除监听事件
        /// </summary>
        public void RemoveEventListener(int key, OnActionHandler handler)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);
            if (lstHandler != null) lstHandler.Remove(handler);
        }
        public void RemoveEventListenerAll(int key)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);
            if (lstHandler != null) lstHandler.Clear();
        }
        #endregion

        #region Dispatch 派发
        /// <summary>
        /// 派发
        /// </summary>
        public void Dispatch(int key, object userData)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);

            if (lstHandler != null && lstHandler.Count > 0)
            {
                for (LinkedListNode<OnActionHandler> curr = lstHandler.First; curr != null; curr = curr.Next)
                {
                    if (curr.Value.Target != null)
                    {
                        curr.Value?.Invoke(userData);
                    }
                }
            }
        }

        public void Dispatch(int key)
        {
            Dispatch(key, null);
        }
        #endregion
    }
}
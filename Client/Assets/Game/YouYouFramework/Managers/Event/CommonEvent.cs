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
        private Dictionary<int, List<OnActionHandler>> actionHandlerDic = new();

        #region AddEventListener 观察者监听事件
        /// <summary>
        /// 观察者监听事件
        /// </summary>
        public void AddEventListener(int key, OnActionHandler handler)
        {
            actionHandlerDic.TryGetValue(key, out List<OnActionHandler> lstHandler);
            if (lstHandler == null)
            {
                lstHandler = new();
                actionHandlerDic[key] = lstHandler;
            }
            lstHandler.Add(handler);
        }
        #endregion

        #region RemoveEventListener 观察者移除监听事件
        /// <summary>
        /// 观察者移除监听事件
        /// </summary>
        public void RemoveEventListener(int key, OnActionHandler handler)
        {
            actionHandlerDic.TryGetValue(key, out List<OnActionHandler> lstHandler);
            lstHandler?.Remove(handler);
        }
        public void RemoveEventListenerAll(int key)
        {
            actionHandlerDic.TryGetValue(key, out List<OnActionHandler> lstHandler);
            lstHandler?.Clear();
        }
        #endregion

        #region Dispatch 派发
        /// <summary>
        /// 派发者派发事件
        /// </summary>
        public void Dispatch(int key, object userData)
        {
            if (actionHandlerDic.TryGetValue(key, out List<OnActionHandler> lstHandler))
            {
                lstHandler.ForEach((OnActionHandler handler) =>
                {
                    if (handler.Target != null)
                    {
                        handler?.Invoke(userData);
                    }
                });
            }
        }
        public void Dispatch(int key)
        {
            Dispatch(key, null);
        }
        #endregion
    }
}
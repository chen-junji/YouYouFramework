using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketEvent 
{
	/// <summary>
	/// 主题原型       所有具体主题都是以该原型为委托类型
	/// </summary>
	/// <param name="P">具体主题参数</param>
	public delegate void OnActionHandler(string jsonData);
	/// <summary>
	/// Key: 主题列表的Key   同个列表里的主题都是同一个Key
	/// Value: 主题列表
	/// </summary>
	private Dictionary<string, LinkedList<OnActionHandler>> dic = new Dictionary<string, LinkedList<OnActionHandler>>();


	#region AddEventListener 观察者监听事件
	/// <summary> 
	/// 观察者监听事件
	/// </summary>
	/// <param name="Key">主题列表的Key</param>
	/// <param name="handler">主题</param>
	public void AddEventListener(string key, OnActionHandler handler)
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
	/// <param name="key">主题列表的Key</param>
	/// <param name="handler">主题</param>
	public void RemoveEventListener(string key, OnActionHandler handler)
	{
		LinkedList<OnActionHandler> lstHandler = null;
		dic.TryGetValue(key, out lstHandler);
		if (lstHandler != null)
		{
			lstHandler.Remove(handler);
			if (lstHandler.Count == 0)
			{
				dic.Remove(key);
			}
		}
	}
    public void RemoveEventListenerAll(string key)
    {
        LinkedList<OnActionHandler> lstHandler = null;
        dic.TryGetValue(key, out lstHandler);
        if (lstHandler != null)
        {
            lstHandler.Clear();
            dic.Remove(key);
        }
    }
	#endregion

	#region Dispatch 发布者派发事件
	/// <summary>
	/// 发布者派发事件
	/// </summary>
	/// <param name="btnKey">主题列表的Key</param>
	/// <param name="jsonData">主题参数</param>
	public void Dispatch(string key, string jsonData)
	{
		LinkedList<OnActionHandler> lstHandler = null;
		dic.TryGetValue(key, out lstHandler);

		if (lstHandler != null && lstHandler.Count > 0)
		{
			for (LinkedListNode<OnActionHandler> curr = lstHandler.First; curr != null; curr = curr.Next)
			{
				curr.Value?.Invoke(jsonData);
			}
		}
	}
	public void Dispatch(string key)
	{
		Dispatch(key, null);
	}
	#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseSprite : MonoBehaviour
{
	private void Start()
	{
		OnInit();
		OnOpen();
	}
	private void Update()
	{
		OnUpdate();
	}
	private void OnDestroy()
	{
		OnBeforDestroy();
	}

	/// <summary>
	/// 克隆时调用
	/// </summary>
	protected virtual void OnInit() { }
	/// <summary>
	/// 从对象池取出时调用
	/// </summary>
	public virtual void OnOpen() { }
	/// <summary>
	/// 退回到对象池时调用
	/// </summary>
	public virtual void OnClose() { }
	/// <summary>
	/// 销毁时调用
	/// </summary>
	protected virtual void OnBeforDestroy() { }
	protected virtual void OnUpdate() { }
}

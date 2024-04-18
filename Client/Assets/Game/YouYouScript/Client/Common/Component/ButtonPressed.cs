using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// 长按按钮判定
/// </summary>
public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	private float delay = 1f;
	private float lastIsDownTime;
	public Action<bool> OnBtnUp;
	public Action OnBtnDown;

	public void OnPointerDown(PointerEventData eventData)
	{
		lastIsDownTime = Time.time;
		OnBtnDown?.Invoke();
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		OnBtnUp?.Invoke(Time.time - lastIsDownTime > delay);
	}
	public void SetRaycastTarget(bool raycastTarget)
	{
		GetComponent<Image>().raycastTarget = raycastTarget;
		GetComponent<Button>().interactable = raycastTarget;
	}
}
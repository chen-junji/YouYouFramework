using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// ʵ�ְ�ť����״̬���ж�
/// </summary>
public class OnButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	// �ӳ�ʱ��
	private float delay = 1f;

	// ��ť���һ���Ǳ���ס״̬ʱ���ʱ��
	private float lastIsDownTime;

	/// <summary>
	/// ��������
	/// </summary>
	public Action<bool> OnDown;

	// ����ť�����º�ϵͳ�Զ����ô˷���
	public void OnPointerDown(PointerEventData eventData)
	{
		lastIsDownTime = Time.time;
	}

	// ����ţ̌���ʱ���Զ����ô˷���
	public void OnPointerUp(PointerEventData eventData)
	{
		// ��ǰʱ�� -  ��ť���һ�α����µ�ʱ�� > �ӳ�ʱ��
		OnDown?.Invoke(Time.time - lastIsDownTime > delay);
	}
}
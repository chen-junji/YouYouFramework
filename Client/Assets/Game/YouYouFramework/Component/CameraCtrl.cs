//===================================================
//��    �ߣ�����  http://www.u3dol.com  QQȺ��87481002
//����ʱ�䣺2020-9-28
//��    ע��
//===================================================
using UnityEngine;
using System.Collections;
using DG.Tweening;
using YouYou;

/// <summary>
/// �����������
/// </summary>
public class CameraCtrl : MonoBehaviour
{
	/// <summary>
	/// �������������
	/// </summary>
	[SerializeField]
	private Transform m_CameraUpAndDown;

	/// <summary>
	/// ��������Ÿ�����
	/// </summary>
	[SerializeField]
	private Transform m_CameraZoomContainer;

	/// <summary>
	/// ���������
	/// </summary>
	[SerializeField]
	private Transform m_CameraContainer;

	[Header("������ת�ٶ�")]
	[SerializeField]
	private int m_RotateSpeed = 80;

	/// <summary>
	/// ���ɿ����Զ���ת�ٶ�
	/// </summary>
	private float m_AutoRotateSpeed;
	private int m_AutoRotateType;

	[HideInInspector]
	/// <summary>
	/// �Ƿ���ק��
	/// </summary>
	public bool IsOnDrag;

	[HideInInspector]
	/// <summary>
	/// ������ק֡����
	/// </summary>
	public float OnDragEndDistance;

	[Header("�����ٶ�")]
	[SerializeField]
	private int m_UpAndDownSpeed = 60;

	[Header("���»����߽�����")]
	[SerializeField]
	private Vector2 m_UpAndDownLimit;

	[Header("�����ٶ�")]
	[SerializeField]
	private int m_ZoomSpeed = 10;

	[Header("���ű߽�����")]
	[SerializeField]
	private Vector2 m_ZoomLimit;

	/// <summary>
	/// �������
	/// </summary>
	[SerializeField]
	public Camera MainCamera;

	void Start()
	{
		GameEntry.CameraCtrl = this;
		Init();
	}

	private void OnDestroy()
	{
	}

	/// <summary>
	/// ������߹ر������
	/// </summary>
	/// <param name="isOpen">�Ƿ���</param>
	public void SetCameraOpen(bool isOpen)
	{
		MainCamera.enabled = isOpen;
	}

	/// <summary>
	/// ��ʼ��
	/// </summary>
	public void Init()
	{
		m_CameraUpAndDown.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(m_CameraUpAndDown.transform.localEulerAngles.z, 35f, 80f));
	}

	/// <summary>
	/// �����������ת
	/// </summary>
	/// <param name="type">0=�� 1=��</param>
	public void SetCameraRotate(int type)
	{
		m_AutoRotateSpeed = m_RotateSpeed;
		m_AutoRotateType = type;
		SetCameraRotateAuto();
	}

	private void SetCameraRotateAuto()
	{
		transform.Rotate(0, m_AutoRotateSpeed * Time.deltaTime * (m_AutoRotateType == 0 ? -1 : 1), 0);
	}

	/// <summary>
	/// ������������� 0=�� 1=��
	/// </summary>
	/// <param name="type"></param>
	public void SetCameraUpAndDown(int type)
	{
		m_CameraUpAndDown.transform.Rotate(0, 0, m_UpAndDownSpeed * Time.deltaTime * (type == 1 ? -1 : 1));
		m_CameraUpAndDown.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(m_CameraUpAndDown.transform.localEulerAngles.z, m_UpAndDownLimit.x, m_UpAndDownLimit.y));
	}

	/// <summary>
	/// ��������� ����
	/// </summary>
	/// <param name="type">0=���� 1=��Զ</param>
	public void SetCameraZoom(int type)
	{
		m_CameraContainer.Translate(Vector3.forward * m_ZoomSpeed * Time.deltaTime * ((type == 1 ? -1 : 1)));
		m_CameraContainer.localPosition = new Vector3(0, 0, Mathf.Clamp(m_CameraContainer.localPosition.z, m_ZoomLimit.x, m_ZoomLimit.y));
	}

	/// <summary>
	/// ʵʱ��������
	/// </summary>
	/// <param name="pos"></param>
	public void AutoLookAt(Vector3 pos)
	{
		m_CameraZoomContainer.LookAt(pos);
	}

	/// <summary>
	/// //����
	/// </summary>
	/// <param name="delay">�ӳ�ʱ��</param>
	/// <param name="duration">����ʱ��</param>
	/// <param name="strength">ǿ��</param>
	/// <param name="vibrato">���</param>
	/// <returns></returns>
	public void CameraShake(float delay = 0, float duration = 0.5f, float strength = 1, int vibrato = 10)
	{
		StartCoroutine(DOCameraShake(delay, duration, strength, vibrato));
	}

	/// <summary>
	/// //����
	/// </summary>
	/// <param name="delay">�ӳ�ʱ��</param>
	/// <param name="duration">����ʱ��</param>
	/// <param name="strength">ǿ��</param>
	/// <param name="vibrato">���</param>
	/// <returns></returns>
	private IEnumerator DOCameraShake(float delay = 0, float duration = 0.5f, float strength = 1, int vibrato = 10)
	{
		yield return new WaitForSeconds(delay);

		m_CameraContainer.DOShakePosition(0.3f, 1f, 100);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 15f);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 14f);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 12f);
	}

	private void Update()
	{
		if (!IsOnDrag && m_AutoRotateSpeed > 0)
		{
			float changeDistance = Mathf.Abs(OnDragEndDistance);
			if (changeDistance > 0)
			{
				m_AutoRotateSpeed -= Mathf.Clamp((100 - Mathf.Clamp(changeDistance, 0, 99)) * Time.deltaTime * 10, 5, 15);
				SetCameraRotateAuto();
			}
		}
	}
}
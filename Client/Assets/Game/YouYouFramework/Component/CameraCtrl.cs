//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2020-9-28
//备    注：
//===================================================
using UnityEngine;
using System.Collections;
using DG.Tweening;
using YouYou;

/// <summary>
/// 摄像机控制器
/// </summary>
public class CameraCtrl : MonoBehaviour
{
	/// <summary>
	/// 控制摄像机上下
	/// </summary>
	[SerializeField]
	private Transform m_CameraUpAndDown;

	/// <summary>
	/// 摄像机缩放父物体
	/// </summary>
	[SerializeField]
	private Transform m_CameraZoomContainer;

	/// <summary>
	/// 摄像机容器
	/// </summary>
	[SerializeField]
	private Transform m_CameraContainer;

	[Header("左右旋转速度")]
	[SerializeField]
	private int m_RotateSpeed = 80;

	/// <summary>
	/// 手松开后自动旋转速度
	/// </summary>
	private float m_AutoRotateSpeed;
	private int m_AutoRotateType;

	[HideInInspector]
	/// <summary>
	/// 是否拖拽中
	/// </summary>
	public bool IsOnDrag;

	[HideInInspector]
	/// <summary>
	/// 结束拖拽帧距离
	/// </summary>
	public float OnDragEndDistance;

	[Header("上下速度")]
	[SerializeField]
	private int m_UpAndDownSpeed = 60;

	[Header("上下滑动边界限制")]
	[SerializeField]
	private Vector2 m_UpAndDownLimit;

	[Header("缩放速度")]
	[SerializeField]
	private int m_ZoomSpeed = 10;

	[Header("缩放边界限制")]
	[SerializeField]
	private Vector2 m_ZoomLimit;

	/// <summary>
	/// 主摄像机
	/// </summary>
	[SerializeField]
	public Camera MainCamera;

	void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
	}

	/// <summary>
	/// 开启或者关闭摄像机
	/// </summary>
	/// <param name="isOpen">是否开启</param>
	public void SetCameraOpen(bool isOpen)
	{
		MainCamera.enabled = isOpen;
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
		m_CameraUpAndDown.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(m_CameraUpAndDown.transform.localEulerAngles.z, 35f, 80f));
	}

	/// <summary>
	/// 设置摄像机旋转
	/// </summary>
	/// <param name="type">0=左 1=右</param>
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
	/// 设置摄像机上下 0=上 1=下
	/// </summary>
	/// <param name="type"></param>
	public void SetCameraUpAndDown(int type)
	{
		m_CameraUpAndDown.transform.Rotate(0, 0, m_UpAndDownSpeed * Time.deltaTime * (type == 1 ? -1 : 1));
		m_CameraUpAndDown.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(m_CameraUpAndDown.transform.localEulerAngles.z, m_UpAndDownLimit.x, m_UpAndDownLimit.y));
	}

	/// <summary>
	/// 设置摄像机 缩放
	/// </summary>
	/// <param name="type">0=拉近 1=拉远</param>
	public void SetCameraZoom(int type)
	{
		m_CameraContainer.Translate(Vector3.forward * m_ZoomSpeed * Time.deltaTime * ((type == 1 ? -1 : 1)));
		m_CameraContainer.localPosition = new Vector3(0, 0, Mathf.Clamp(m_CameraContainer.localPosition.z, m_ZoomLimit.x, m_ZoomLimit.y));
	}

	/// <summary>
	/// 实时看着主角
	/// </summary>
	/// <param name="pos"></param>
	public void AutoLookAt(Vector3 pos)
	{
		m_CameraZoomContainer.LookAt(pos);
	}

	/// <summary>
	/// //震屏
	/// </summary>
	/// <param name="delay">延迟时间</param>
	/// <param name="duration">持续时间</param>
	/// <param name="strength">强度</param>
	/// <param name="vibrato">震幅</param>
	/// <returns></returns>
	public void CameraShake(float delay = 0, float duration = 0.5f, float strength = 1, int vibrato = 10)
	{
		StartCoroutine(DOCameraShake(delay, duration, strength, vibrato));
	}

	/// <summary>
	/// //震屏
	/// </summary>
	/// <param name="delay">延迟时间</param>
	/// <param name="duration">持续时间</param>
	/// <param name="strength">强度</param>
	/// <param name="vibrato">震幅</param>
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
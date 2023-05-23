using UnityEngine;
using System.Collections;
using DG.Tweening;
using YouYou;

/// <summary>
/// 摄像机控制器
/// </summary>
public class CameraCtrl : SingletonMono<CameraCtrl>
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
    public Camera MainCamera;

    //当前实际值
    private float distance = 20f;
    private float rotateX = 0.0f;
    private float rotateY = 0.0f;
    //平滑目标值
    private float targetX = 0f;
    private float targetY = 0f;
    private float targetDistance = 0f;

    //当前平滑阻尼速度
    private float xVelocity = 1f;
    private float yVelocity = 1f;
    private float zoomVelocity = 1f;


    void Start()
    {
        targetY = m_UpAndDownLimit.x;
        targetDistance = m_ZoomLimit.x;
    }
    private void LateUpdate()
    {
        //这四行可以搬到外部调用
        SetCameraRotateX(GameEntry.Input.GetAxis(InputName.MouseX));
        SetCameraRotateY(GameEntry.Input.GetAxis(InputName.MouseY));
        if (Input.GetKey(KeyCode.K)) SetCameraDistance(Time.deltaTime * 10);
        else if (Input.GetKey(KeyCode.L)) SetCameraDistance(-Time.deltaTime * 10);


        //左右旋转 计算阻尼并刷新位置
        rotateX = Mathf.SmoothDamp(rotateX, targetX, ref xVelocity, 0.2f);
        transform.localEulerAngles = new Vector3(0, rotateX, 0);

        //上下旋转 计算阻尼并刷新位置
        rotateY = Mathf.SmoothDamp(rotateY, targetY, ref yVelocity, 0.2f);
        m_CameraUpAndDown.transform.localEulerAngles = new Vector3(rotateY, 0, 0);

        //前后缩放 计算阻尼并刷新位置
        distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, 0.5f);
        m_CameraZoomContainer.localPosition = new Vector3(0, 0, distance);
    }

    /// <summary>
    /// 设置摄像机 左右旋转 增加偏移量
    /// </summary>
    public void SetCameraRotateX(float value)
    {
        if (Mathf.Abs(value) > 0.01f) targetX += value * m_RotateSpeed * 0.01f;
    }
    /// <summary>
    /// 设置摄像机 上下旋转 增加偏移量
    /// </summary>
    public void SetCameraRotateY(float value)
    {
        if (Mathf.Abs(value) > 0.01f) targetY -= value * m_UpAndDownSpeed * 0.01f;
        targetY = ClampAngle(targetY, m_UpAndDownLimit.x, m_UpAndDownLimit.y);
    }
    /// <summary>
    /// 设置摄像机 前后缩放 增加偏移量
    /// </summary>
    public void SetCameraDistance(float value)
    {
        if (Mathf.Abs(value) > 0.01f) targetDistance += value * m_ZoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, m_ZoomLimit.x, m_ZoomLimit.y);
    }

    #region ClampAngle 限制旋转角度的极限范围
    /// <summary>
    /// 限制旋转角度的极限范围
    /// </summary>
    /// <param name="angle">当前旋转角度</param>
    /// <param name="min">极限负旋转</param>
    /// <param name="max">极限正旋转</param>
    /// <returns></returns>
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    #endregion

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
        StartCoroutine(DOCameraShake());
        IEnumerator DOCameraShake()
        {
            yield return new WaitForSeconds(delay);
            m_CameraContainer.DOShakePosition(duration, strength, vibrato);
        }
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

}
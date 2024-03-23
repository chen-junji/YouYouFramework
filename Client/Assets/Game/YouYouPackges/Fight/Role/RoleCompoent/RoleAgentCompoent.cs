using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class RoleAgentCompoent : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }

    /// <summary>
    /// 路经点
    /// </summary>
    private Vector3[] m_VectorPath;

    /// <summary>
    /// 当前路经点索引
    /// </summary>
    private int m_CurrPointIndex;

    /// <summary>
    /// 转身完毕
    /// </summary>
    private bool m_TurnComplete = false;

    private Vector3 endPos;
    private Vector3 beginPos;
    private Vector3 dir;
    private Vector3 rotation;
    [SerializeField] float runSpeed = 10; //速度
    private float modifyRunSpeed = 10;//修正速度

    private NavMeshPath path;

    public Action MoveTargetEnd;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }
    void Update()
    {
        if (m_VectorPath == null)
        {
            return;
        }

        //如果整个路径走完了 切换待机
        if (m_CurrPointIndex >= m_VectorPath.Length)
        {
            m_VectorPath = null;
            MoveTargetEnd?.Invoke();
            return;
        }

        if (!m_TurnComplete)
        {
            endPos = m_VectorPath[m_CurrPointIndex];
            beginPos = m_VectorPath[m_CurrPointIndex - 1];

            dir = (endPos - beginPos).normalized;

            rotation = dir;
            //立刻转身
            rotation.y = 0;
            transform.rotation = Quaternion.LookRotation(rotation);

            m_TurnComplete = true;
        }
        Agent.Move(dir * Time.deltaTime * modifyRunSpeed);

        //判断是否应该向下一个点移动
        float dis = Vector3.Distance(transform.position, beginPos);

        //当到达临时目标点了
        if (dis >= Vector3.Distance(endPos, beginPos))
        {
            //位置修正
            SetPostionAndRotation(endPos, transform.rotation);

            m_TurnComplete = false;
            m_CurrPointIndex++;
        }
    }

    public void SetPostionAndRotation(Vector3 pos, Quaternion rot)
    {
        Agent.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        Agent.enabled = true;
    }

    public NavMeshPathStatus ClickMove(Vector3 targetPos)
    {
        m_CurrPointIndex = 1;
        m_TurnComplete = false;
        //runSpeed = 10;
        modifyRunSpeed = runSpeed;

        //计算路径
        Agent.CalculatePath(targetPos, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            m_VectorPath = path.corners;
        }
        return path.status;
    }
    public void ClickMoveStop()
    {
        m_VectorPath = null;
    }

    public void JoystickMove(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        //1.设置辅助器位置
        //GameEntry.Data.RoleDataMgr.CurrPlayerMoveHelper.transform.position = new Vector3(transform.position.x + dir.x, transform.position.y, transform.position.z + dir.y);

        ////2.让辅助器进行旋转
        //GameEntry.Data.RoleDataMgr.CurrPlayerMoveHelper.transform.RotateAround(transform.position, Vector3.up, CameraCtrl.Instance.transform.localEulerAngles.y);//- 90

        ////3.得到真正要移动的方向
        //Vector3 direction = GameEntry.Data.RoleDataMgr.CurrPlayerMoveHelper.transform.position - transform.position;

        //direction.Normalize();//归一化，确保力度一致
        //direction = direction * Time.deltaTime * modifyRunSpeed;
        //transform.rotation = Quaternion.LookRotation(direction);
        //Agent.Move(direction);

    }
}

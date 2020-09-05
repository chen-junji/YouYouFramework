//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2016-06-10 20:36:23
//备    注：
//===================================================
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class UISelectRoleDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //开始拖拽的位置
    private Vector2 m_DragBeginPos = Vector2.zero;

    //结束拖拽的位置
    private Vector2 m_DragEndPos = Vector2.zero;

    public delegate void OnDragingHandler(int dir);
    /// <summary>
    /// 拖拽中
    /// </summary>
    public OnDragingHandler OnDraging;


    public delegate void OnDragCompleteHandler(int dir);
    /// <summary>
    /// 拖拽委托 0=左 1=右
    /// </summary>
    public OnDragCompleteHandler OnDragComplete;


    public delegate void OnDoubleClickHandler();
    /// <summary>
    /// 双击
    /// </summary>
    public OnDoubleClickHandler OnDoubleClick;

    void Start()
    {

    }

    //计时器，在一定的时间内双击有效  
    private float m_Time = 0f;
    //计数器  
    private int m_ClickCount = 0;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            m_ClickCount++;
            //当第一次点击鼠标，启动计时器  
            if (m_ClickCount == 1)
            {
                m_Time = Time.time;
            }
            //当第二次点击鼠标，且时间间隔满足要求时双击鼠标  
            if (2 == m_ClickCount && Time.time - m_Time <= 0.5f)
            {
                if (OnDoubleClick != null) OnDoubleClick();
                m_ClickCount = 0;
            }
            if (Time.time - m_Time > 0.5f)
            {
                m_ClickCount = 0;
            }
        }
    }

    void OnDestroy()
    {
        OnDragComplete = null;
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_DragBeginPos = eventData.position;
    }

    /// <summary>
    /// 拖拽中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        m_DragEndPos = eventData.position;

        float x = m_DragBeginPos.x - m_DragEndPos.x;
        if (x > 1)
        {
            //Debug.Log("向左拖拽");
            if (OnDraging != null)
            {
                OnDraging(0);
            }
        }
        else if (x < -1)
        {
            //Debug.Log("向右拖拽");
            if (OnDraging != null)
            {
                OnDraging(1);
            }
        }
    }

    /// <summary>
    /// 结束拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        m_DragEndPos = eventData.position;

        float x = m_DragBeginPos.x - m_DragEndPos.x;

        //这个20是容错范围
        if (x > 20)
        {
            if (OnDragComplete != null)
            {
                OnDragComplete(0);
            }
        }
        else if (x < -20)
        {
            if (OnDragComplete != null)
            {
                OnDragComplete(1);
            }
        }
    }
}
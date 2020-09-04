using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/// <summary>
///
/// </summary>
public class UISelectRoleDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    ///
    /// </summary>
    private Vector2 m_DragBeginPos = Vector2.zero;

    /// <summary>
    ///
    /// </summary>
    private Vector2 m_DragEndPos = Vector2.zero;

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnDragingHandler(int dir);
    public OnDragingHandler OnDraging;

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnDragCompleteHandler(int dir);
    public OnDragingHandler OnDragComplete;

    /// <summary>
    /// 
    /// </summary>
    public delegate void OnDoubleHandler(int dir);
    public OnDragingHandler OnDoubleClick;

    void OnDestroy()
    {
        OnDragComplete = null;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_DragBeginPos = eventData.position;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        m_DragEndPos = eventData.position;

        float x = m_DragBeginPos.x - m_DragEndPos.x;
        if (x > 1)
        {
            if (OnDraging != null) OnDraging(0);
        }else if (x < -1)
        {
            if (OnDraging != null) OnDraging(1);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        m_DragEndPos = eventData.position;

        //
        float x = m_DragBeginPos.x - m_DragEndPos.x;
        if (x > 20)//
        {
            
            if (OnDragComplete != null) OnDragComplete(0);
        }
        else if (x < -20)//
        {
            if (OnDragComplete != null) OnDragComplete(1);
        }
    }
}
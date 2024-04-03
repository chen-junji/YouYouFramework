using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using YouYouFramework;

public class UIRollingScroller : UIScroller, IDragHandler, IEndDragHandler
{
    private float contentOrignHigh;

    public Action<Vector3> OnHaveNewMsg;
    public Action OnNewPos;

    //自动滑动到最下方
    private bool isDraging = false;

    public override void Start()
    {
        base.Start();
        contentOrignHigh = _content.rect.height;
    }
    public void OnDrag(PointerEventData eventData)
    {
        isDraging = true;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
    }

    /// <summary>
    /// 聊天内容自动滚动到最新消息的位置
    /// </summary>
    /// <param name="number"></param>
    /// <param name="isReLoadData">是否重新加载</param>
    public void RollIngShowMsg(int number, bool isReLoadData = false)
    {
        //float temphight = itemHigh * number;
         GameEntry.Log(LogCategory.UI,"number " + number + " viewCount " + viewCount);

        if (viewCount > number)
        {
            //内容不满一页
            ResetScroller();
            if (contentOrignHigh < itemPrefab.rect.height * number)
            {
                ChangeConentPos(isReLoadData);
            }
        }
        else
        {
            if (isReLoadData)
            {
                ResetScroller();
                DataCount = number;
            }
            ChangeConentPos(isReLoadData);
        }
    }

    private void ChangeConentPos(bool isReLoadData)
    {
        //想要看到最新的消息，Content的Y应该要处在的坐标
        float tempY = Mathf.Floor(_content.rect.height - contentOrignHigh);

        //求Content目标位置与当前位置的距离
        float targetYPos = tempY - _content.anchoredPosition3D.y;

        // 上拉超过半屏则不会自动滚到最新消息的位置
        float autoRollingHigh = contentOrignHigh * 0.9f;
        // 自动滚到最新消息的位置
        if (targetYPos < autoRollingHigh && isDraging == false)
        {
            _content.DOAnchorPos3D(new Vector3(0f, tempY, 0f), 0.5f);
        }
        else if (isReLoadData)
        {
            //重新加载的记录则直接跑到最新消息的位置
            _content.anchoredPosition3D = new Vector3(0f, tempY, 0f);
        }
        else
        {
            OnHaveNewMsg?.Invoke(new Vector3(0f, tempY, 0f));
            return;
        }
        OnNewPos?.Invoke();
    }


}

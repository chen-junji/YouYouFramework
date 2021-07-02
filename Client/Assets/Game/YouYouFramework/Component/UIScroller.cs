using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]//脚本依赖
public class UIScroller : MonoBehaviour
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }
    public Arrangement _movement = Arrangement.Horizontal;

    /// <summary>
    /// 行间距
    /// </summary>
    [Range(0, 100)] [SerializeField] float cellPadiding = 0;

    //Item的宽高
    private float cellWidth;
    private float cellHeight;
    //默认加载的行数，一般比可显示行数大2~3行
    int viewCount;

    [SerializeField] RectTransform itemPrefab;
    ScrollRect ScrollRect;
    RectTransform _content;

    private int _index = -1;
    private List<UIScrollIndex> _itemList;
    private int _dataCount;

    private Queue<UIScrollIndex> _unUsedQueue;  //将未显示出来的Item存入未使用队列里面，等待需要使用的时候直接取出

    //第一步 监听这个委托
    public Action<int, GameObject> OnItemCreate;
    /// <summary>
    /// 第二步 设置 DataCount 总数量 需要提前设置
    /// </summary>
    public int DataCount
    {
        get { return _dataCount; }
        set
        {
            _dataCount = value;
            UpdateTotalWidth();
            ResetScroller();
        }
    }

    void Start()
    {
        _itemList = new List<UIScrollIndex>();
        _unUsedQueue = new Queue<UIScrollIndex>();

        ScrollRect = GetComponent<ScrollRect>();
        _content = ScrollRect.content;

        cellWidth = itemPrefab.rect.width;
        cellHeight = itemPrefab.rect.height;

        if (_movement == Arrangement.Horizontal)
        {
            viewCount = Mathf.CeilToInt(ScrollRect.viewport.rect.width / cellWidth * 1.2f);
        }
        else
        {
            viewCount = Mathf.CeilToInt(ScrollRect.viewport.rect.height / cellHeight * 1.2f);
        }

        ScrollRect.onValueChanged.AddListener(OnValueChange);

        OnValueChange(Vector2.zero);
    }

    private void OnDestroy()
    {
        itemPrefab = null;
        _content = null;

        _itemList = null;
        _unUsedQueue = null;
        OnItemCreate = null;
    }

    /// <summary>
    /// 第三步 刷新视图
    /// </summary>
    public void ResetScroller()
    {
        _index = -1;
        UIScrollIndex[] arr = _content.GetComponentsInChildren<UIScrollIndex>();
        for (int i = 0; i < arr.Length; i++)
        {
            DestroyImmediate(arr[i].gameObject);
        }
        arr = null;

        if (_itemList != null) _itemList.Clear();
        if (_unUsedQueue != null) _unUsedQueue.Clear();
        _content.anchoredPosition = new Vector2(0, 1f);
        OnValueChange(Vector2.zero);
    }
    public void OnValueChange(Vector2 pos)
    {
        if (_itemList == null) return;
        int index = GetPosIndex();
        if (_index != index && index > -1)
        {
            _index = index;
            for (int i = _itemList.Count; i > 0; i--)
            {
                UIScrollIndex item = _itemList[i - 1];
                if (item.Index < index || (item.Index >= (index + viewCount)))
                {
                    _itemList.Remove(item);
                    _unUsedQueue.Enqueue(item);
                }
            }
            for (int i = _index; i < (_index + viewCount); i++)
            {
                if (i < 0) continue;
                if (i > _dataCount - 1) continue;
                bool isOk = false;
                foreach (UIScrollIndex item in _itemList)
                {
                    if (item.Index == i) isOk = true;
                }
                if (isOk) continue;
                CreateItem(i);
            }
        }
    }

    private void CreateItem(int index)
    {
        UIScrollIndex itemBase;
        if (_unUsedQueue.Count > 0)
        {
            itemBase = _unUsedQueue.Dequeue();
        }
        else
        {
            itemBase = Instantiate(itemPrefab.gameObject, _content).AddComponent<UIScrollIndex>();
        }
        itemBase.SetUI(index, GetPosition(index));
        _itemList.Add(itemBase);
        OnItemCreate?.Invoke(index, itemBase.gameObject);
    }

    #region GetPosIndex
    /// <summary>
    /// 获取最上位置的索引
    /// </summary>
    /// <returns></returns>
    private int GetPosIndex()
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                {
                    return Mathf.FloorToInt(_content.anchoredPosition.x / -(cellWidth + cellPadiding));
                }
            case Arrangement.Vertical:
                {
                    return Mathf.FloorToInt(_content.anchoredPosition.y / (cellHeight + cellPadiding));
                }
        }
        return 0;
    }
    #endregion

    #region GetPosition
    /// <summary>
    /// 根据索引号 获取当前item的位置
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private Vector3 GetPosition(int i)
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                return new Vector3(i * (cellWidth + cellPadiding), 0f, 0f);
            case Arrangement.Vertical:
                return new Vector3(0f, i * -(cellHeight + cellPadiding), 0f);
        }
        return Vector3.zero;
    }
    #endregion

    #region UpdateTotalWidth 
    /// <summary>
    /// 这个方法的目的 就是根据总数量 行列 来计算content的真正宽度或者高度
    /// </summary>
    private void UpdateTotalWidth()
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                _content.sizeDelta = new Vector2(cellWidth * _dataCount + cellPadiding * (_dataCount - 1), _content.sizeDelta.y);
                break;
            case Arrangement.Vertical:
                _content.sizeDelta = new Vector2(_content.sizeDelta.x, cellHeight * _dataCount + cellPadiding * (_dataCount - 1));
                break;
        }
    }
    #endregion
}

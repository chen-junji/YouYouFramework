using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class UIScroller : MonoBehaviour
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }
    /// <summary>
    /// 滚动方向
    /// </summary>
    public Arrangement _movement = Arrangement.Horizontal;

    /// <summary>
    /// 行间距
    /// </summary>
    [Range(0, 100)] [SerializeField] public float cellPadiding = 0;

    /// <summary>
    /// Item的宽
    /// </summary>
    private float itemWidth;

    /// <summary>
    /// Item的高
    /// </summary>
    private float itemHeight;

    /// <summary>
    /// 默认加载的行数，一般比可显示行数大2~3行
    /// </summary>
    public int viewCount;

    public RectTransform itemPrefab;
    public ScrollRect ScrollRect;

    /// <summary>
    /// 最上位置的索引
    /// </summary>
    private int _index = -1;
    [HideInInspector]
    public List<UIScrollIndex> _itemList;

    /// <summary>
    /// 设置 DataCount 总数量
    /// </summary>
    private int _dataCount;

    /// <summary>
    /// 将未显示出来的Item存入未使用队列里面，等待需要使用的时候直接取出
    /// </summary>
    private Queue<UIScrollIndex> _unUsedItemQueue;

    /// <summary>
    /// 第一步 监听这个委托
    /// </summary>
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
            //ResetScroller();
        }
    }

    public virtual void Awake()
    {
        _itemList = new List<UIScrollIndex>();
        _unUsedItemQueue = new Queue<UIScrollIndex>();

        itemWidth = itemPrefab.rect.width;
        itemHeight = itemPrefab.rect.height;

        if (_movement == Arrangement.Horizontal)
        {
            viewCount = Mathf.CeilToInt(ScrollRect.viewport.rect.width / itemWidth * 1.2f);
        }
        else
        {
            viewCount = Mathf.CeilToInt(ScrollRect.viewport.rect.height / itemHeight * 1.2f);
        }
        ScrollRect.onValueChanged.AddListener(OnValueChange);
    }

    public virtual void Start()
    {
        OnValueChange(Vector2.zero);
    }

    public virtual void OnDestroy()
    {
        itemPrefab = null;
        ScrollRect.content = null;

        _itemList = null;
        _unUsedItemQueue = null;
        OnItemCreate = null;
    }

    /// <summary>
    /// 第三步 刷新视图（更新视图或者消息不满一页的使用使用）
    /// </summary>
    public void ResetScroller()
    {
        //当页面数据不满一页的时候，要注意修改 _index的值 为-1 
        _index = -1;

        UIScrollIndex[] arr = ScrollRect.content.GetComponentsInChildren<UIScrollIndex>();
        for (int i = 0; i < arr.Length; i++)
        {
            DestroyImmediate(arr[i].gameObject);
        }
        arr = null;

        if (_itemList != null) _itemList.Clear();
        if (_unUsedItemQueue != null) _unUsedItemQueue.Clear();
        ScrollRect.content.anchoredPosition = new Vector2(0, 1f);
        OnValueChange(Vector2.zero);
    }

    /// <summary>
    /// Scroll View 滚动时，创建/修改预设预设坐标
    /// </summary>
    /// <param name="pos"></param>
    public void OnValueChange(Vector2 pos)
    {
        if (_itemList == null) return;
        int index = GetPosIndex();

        if (_index != index && index > -1)
        {
            _index = index;
            //找出视野范围外的预设，加入未使用队列
            for (int i = _itemList.Count; i > 0; i--)
            {
                UIScrollIndex item = _itemList[i - 1];
                if (item.Index < index || (item.Index >= (index + viewCount)))
                {
                    _itemList.Remove(item);
                    _unUsedItemQueue.Enqueue(item);
                }
            }

            //创建新预设/改变预设的坐标
            for (int i = _index; i < (_index + viewCount); i++)
            {
                if (i < 0) continue;
                if (_dataCount - 1 < i) continue;

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
        if (_unUsedItemQueue.Count > 0)
        {
            itemBase = _unUsedItemQueue.Dequeue();
        }
        else
        {
            itemBase = Instantiate(itemPrefab.gameObject, ScrollRect.content).AddComponent<UIScrollIndex>();
            if (itemBase.gameObject.activeInHierarchy == false)
            {
                itemBase.gameObject.SetActive(true);
            }
        }
        //if (index < 7 || index == 7)
        //{
        //    GameEntry.Log(itemBase.name);
        //    itemBase.name = index.ToString();
        //}
        //更新坐标
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
                    return Mathf.FloorToInt(ScrollRect.content.anchoredPosition.x / -(itemWidth + cellPadiding));
                }
            case Arrangement.Vertical:
                {
                    return Mathf.FloorToInt(ScrollRect.content.anchoredPosition.y / (itemHeight + cellPadiding));
                }
        }
        return 0;
    }
    #endregion

    #region GetPosition
    /// <summary>
    /// 根据索引号 计算item的新坐标
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private Vector3 GetPosition(int i)
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                return new Vector3(i * (itemWidth + cellPadiding), 0f, 0f);
            case Arrangement.Vertical:
                return new Vector3(0f, i * -(itemHeight + cellPadiding), 0f);
        }
        return Vector3.zero;
    }
    #endregion

    #region UpdateTotalWidth 
    /// <summary>
    /// 这个方法的目的 就是根据总数量 行列 刷新content的宽度或者高度
    /// </summary>
    private void UpdateTotalWidth()
    {
        int _dataCountValue = _dataCount - 1;
        if (_dataCountValue < 0)
        {
            _dataCountValue = 0;
        }
        switch (_movement)
        {
            case Arrangement.Horizontal:
                ScrollRect.content.sizeDelta = new Vector2(itemWidth * _dataCount + cellPadiding * _dataCountValue, ScrollRect.content.sizeDelta.y);
                break;
            case Arrangement.Vertical:
                ScrollRect.content.sizeDelta = new Vector2(ScrollRect.content.sizeDelta.x, itemHeight * _dataCount + cellPadiding * _dataCountValue);
                break;
        }
    }

    #endregion
}

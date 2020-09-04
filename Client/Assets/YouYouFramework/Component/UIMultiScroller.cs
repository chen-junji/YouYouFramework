using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIMultiScroller : MonoBehaviour
{
    public enum Arrangement { Horizontal, Vertical, }
    public Arrangement _movement = Arrangement.Horizontal;

    /// <summary>
    /// 单行或单列的Item数量
    /// </summary>
    [Range(1, 20)]
    public int maxPerLine = 5;

    /// <summary>
    /// 行间距X
    /// </summary>
    [Range(0, 100)]
    public int spacingX = 5;

    /// <summary>
    /// 行间距Y
    /// </summary>
    [Range(0, 100)]
    public int spacingY = 5;


    //Item的宽高
    public int cellWidth = 500;
    public int cellHeight = 100;

    //默认加载的行数，一般比可显示行数大2~3行
    [Range(0, 20)]
    public int viewCount = 6;
    public GameObject itemPrefab;
    public RectTransform _content;

    private int _index = -1;
    private List<UIMultiScrollIndex> _itemList;
    private int _dataCount;

    private Queue<UIMultiScrollIndex> _unUsedQueue;  //将未显示出来的Item存入未使用队列里面，等待需要使用的时候直接取出

    /// <summary>
    /// Item创建
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="obj">创建的物体</param>
    public delegate void OnItemCreateHandler(int index, GameObject obj);

    //第二步 在Lua中 添加监听这个委托
    public OnItemCreateHandler OnItemCreate;

    #region 第一步 设置 DataCount 总数量 需要提前设置
    /// <summary>
    /// 总数量
    /// </summary>
    public int DataCount
    {
        get { return _dataCount; }
        set
        {
            _dataCount = value;
            UpdateTotalWidth();
        }
    }
    #endregion

    void Start()
    {
        _itemList = new List<UIMultiScrollIndex>();
        _unUsedQueue = new Queue<UIMultiScrollIndex>();
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

    public void ResetScroller()
    {
        _index = -1;
        UIMultiScrollIndex[] arr = _content.GetComponentsInChildren<UIMultiScrollIndex>();
        for (int i = 0; i < arr.Length; i++)
        {
            DestroyImmediate(arr[i].gameObject);
        }
        arr = null;

        if (_itemList != null)
        {
            _itemList.Clear();
        }

        if (_unUsedQueue != null)
        {
            _unUsedQueue.Clear();
        }
        _content.anchoredPosition = new Vector2(0, 1f);
        OnValueChange(Vector2.zero);
    }

    #region OnValueChange
    public void OnValueChange(Vector2 pos)
    {
        if (_itemList == null) return;

        int index = GetPosIndex();

        if (_index != index && index > -1)
        {
            _index = index;
            for (int i = _itemList.Count; i > 0; i--)
            {
                UIMultiScrollIndex item = _itemList[i - 1];
                if (item.Index < index * maxPerLine || (item.Index >= (index + viewCount) * maxPerLine))
                {
                    _itemList.Remove(item);
                    _unUsedQueue.Enqueue(item);
                }
            }
            for (int i = _index * maxPerLine; i < (_index + viewCount) * maxPerLine; i++)
            {
                if (i < 0) continue;
                if (i > _dataCount - 1) continue;
                bool isOk = false;
                foreach (UIMultiScrollIndex item in _itemList)
                {
                    if (item.Index == i) isOk = true;
                }
                if (isOk) continue;
                CreateItem(i);
            }
        }
    }
    #endregion

    private void CreateItem(int index)
    {
        UIMultiScrollIndex itemBase;
        if (_unUsedQueue.Count > 0)
        {
            itemBase = _unUsedQueue.Dequeue();
            itemBase.Scroller = this;
            itemBase.Index = index;
        }
        else
        {
            itemBase = GameUtil.AddChild(_content, itemPrefab).GetComponent<UIMultiScrollIndex>();
            itemBase.Scroller = this;
            itemBase.Index = index;
        }

        _itemList.Add(itemBase);

        if (OnItemCreate != null)
        {
            OnItemCreate(index, itemBase.gameObject);
        }
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
                    return Mathf.FloorToInt(_content.anchoredPosition.x / -(cellWidth + spacingY));
                }
            case Arrangement.Vertical:
                {
                    return Mathf.FloorToInt(_content.anchoredPosition.y / (cellHeight + spacingY));
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
    public Vector3 GetPosition(int i)
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                return new Vector3(cellWidth * (i / maxPerLine), -(cellHeight + spacingY) * (i % maxPerLine), 0f);
            case Arrangement.Vertical:
                return new Vector3(cellWidth * (i % maxPerLine) + (i % 2 == 1 ? spacingX : 0), -(cellHeight + spacingY) * (i / maxPerLine), 0f);
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
        int lineCount = Mathf.CeilToInt((float)_dataCount / maxPerLine);
        switch (_movement)
        {
            case Arrangement.Horizontal:
                _content.sizeDelta = new Vector2(cellWidth * lineCount + spacingY * (lineCount - 1), _content.sizeDelta.y);
                break;
            case Arrangement.Vertical:
                _content.sizeDelta = new Vector2(_content.sizeDelta.x, cellHeight * lineCount + spacingY * (lineCount - 1));
                break;
        }
    }
    #endregion
}

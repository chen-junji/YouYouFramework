//-------------------------------
//该Demo由风冻冰痕所写
//http://icemark.cn/blog
//转载请说明出处
//-------------------------------
using UnityEngine;
using System.Collections.Generic;

public class UIScroller : MonoBehaviour
{
    public enum Arrangement { Horizontal, Vertical, }
    public Arrangement _movement = Arrangement.Horizontal;
    //Item之间的距离
    [Range(0, 20)]
    public int cellPadiding = 5;
    //Item的宽高
    public int cellWidth = 500;
    public int cellHeight = 100;
    //默认加载的Item个数，一般比可显示个数大2~3个
    [Range(0, 20)]
    public int viewCount = 6;
    public GameObject itemPrefab;
    public RectTransform _content;

    private int _index = -1;
    private List<UIScrollIndex> _itemList;
    private int _dataCount;

    private Queue<UIScrollIndex> _unUsedQueue;  //将未显示出来的Item存入未使用队列里面，等待需要使用的时候直接取出

    void Start()
    {
        _itemList = new List<UIScrollIndex>();
        _unUsedQueue = new Queue<UIScrollIndex>();
        DataCount = 100;
        OnValueChange(Vector2.zero);
    }

    public void OnValueChange(Vector2 pos)
    {
        int index = GetPosIndex();
        if (_index != index && index > -1)
        {
            _index = index;
            for (int i = _itemList.Count; i > 0; i--)
            {
                UIScrollIndex item = _itemList[i - 1];
                if (item.Index < index || (item.Index >= index + viewCount))
                {
                    //GameObject.Destroy(item.gameObject);
                    _itemList.Remove(item);
                    _unUsedQueue.Enqueue(item);
                }
            }
            for (int i = _index; i < _index + viewCount; i++)
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

    /// <summary>
    /// 提供给外部的方法，添加指定位置的Item
    /// </summary>
    public void AddItem(int index)
    {
        if (index > _dataCount)
        {
            Debug.LogError("添加错误:" + index);
            return;
        }
        AddItemIntoPanel(index);
        DataCount += 1;
    }

    /// <summary>
    /// 提供给外部的方法，删除指定位置的Item
    /// </summary>
    public void DelItem(int index)
    {
        if (index < 0 || index > _dataCount - 1)
        {
            Debug.LogError("删除错误:" + index);
            return;
        }
        DelItemFromPanel(index);
        DataCount -= 1;
    }

    private void AddItemIntoPanel(int index)
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            UIScrollIndex item = _itemList[i];
            if (item.Index >= index) item.Index += 1;
        }
        CreateItem(index);
    }

    private void DelItemFromPanel(int index)
    {
        int maxIndex = -1;
        int minIndex = int.MaxValue;
        for (int i = _itemList.Count; i > 0; i--)
        {
            UIScrollIndex item = _itemList[i - 1];
            if (item.Index == index)
            {
                GameObject.Destroy(item.gameObject);
                _itemList.Remove(item);
            }
            if (item.Index > maxIndex)
            {
                maxIndex = item.Index;
            }
            if (item.Index < minIndex)
            {
                minIndex = item.Index;
            }
            if (item.Index > index)
            {
                item.Index -= 1;
            }
        }
        if (maxIndex < DataCount - 1)
        {
            CreateItem(maxIndex);
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
            itemBase = GameUtil.AddChild(_content, itemPrefab).GetComponent<UIScrollIndex>();
        }

        itemBase.Scroller = this;
        itemBase.Index = index;
        _itemList.Add(itemBase);
    }

    private int GetPosIndex()
    {
        switch (_movement)
        {
            case Arrangement.Horizontal:
                return Mathf.FloorToInt(_content.anchoredPosition.x / -(cellWidth + cellPadiding));
            case Arrangement.Vertical:
                return Mathf.FloorToInt(_content.anchoredPosition.y / (cellHeight + cellPadiding));
        }
        return 0;
    }

    public Vector3 GetPosition(int i)
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

    public int DataCount
    {
        get { return _dataCount; }
        set
        {
            _dataCount = value;
            UpdateTotalWidth();
        }
    }

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
}

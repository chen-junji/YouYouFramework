using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class PageView : MonoBehaviour
{
	public Transform itemPool;
	public Transform itemPanel;
	public GameObject itemPrefab;
	public Text txtPage;
	public UISelectRoleDragView m_UISelectRoleDragView;

	public Transform pageDotParent;
	public GameObject pageDot;

	/// <summary>
	/// 总数量
	/// </summary>
	private int m_TotalCount;

	/// <summary>
	/// 一共有多少行
	/// </summary>
	public int RowCount;

	/// <summary>
	/// 一共有多少列
	/// </summary>
	public int ColumnCount;

	/// <summary>
	/// 单元格宽度
	/// </summary>
	public int ItemWidth = 100;

	/// <summary>
	/// 每页数量
	/// </summary>
	public int PageSize
	{
		get { return RowCount * ColumnCount; }
	}

	private int m_TotalPage; //最大页数
	private int m_PageIndex; //当前页
	private List<Transform> m_NewItemList;
	private List<Transform> m_OldItemList;
	private bool m_IsPlaying = false;    //标记是否正在播放缓动动画
	private Queue<GameObject> m_Queue;

	public delegate void OnItemCreateHandler(int dataIndex, GameObject obj);
	public OnItemCreateHandler OnItemCreate;

	private List<Image> m_DotList;

	void Start()
	{

		m_OldItemList = new List<Transform>();
		m_Queue = new Queue<GameObject>();
		m_NewItemList = new List<Transform>();

		m_UISelectRoleDragView.OnDragComplete = (int type) =>
		{
			//0=左 1=右
			OnPageButtonCLick(type == 0 ? "Next" : "Prev");
		};

		//首次创建
		CreateItemGrid(0, true);
	}

	public void InitData(int totalCount)
	{
		m_TotalCount = totalCount;

		//计算最大页数
		m_TotalPage = (int)Mathf.Ceil((float)m_TotalCount / PageSize);
		if (m_TotalPage == 0) m_TotalPage = 1;

		m_PageIndex = 1;
		InitDotPage();

		CreateItemGrid(0, true, true);
	}

	private void InitDotPage()
	{
		//克隆分页的点
		if (m_DotList == null)
		{
			m_DotList = new List<Image>();
		}
		else
		{
			for (int i = 0; i < m_DotList.Count; i++)
			{
				DestroyImmediate(m_DotList[i].gameObject);
			}
			m_DotList.Clear();
		}

		float pageDotWidth = 20f; //分页点的间距
		float beginX = -1 * (((m_TotalPage * 0.5f) * pageDotWidth) - (pageDotWidth * 0.5f));

		for (int i = 0; i < m_TotalPage; i++)
		{
			GameObject go = GameUtil.AddChild(pageDotParent, pageDot); //创建
			go.transform.localPosition = new Vector3(beginX + (pageDotWidth * i), 0, 0);
			m_DotList.Add(go.GetComponent<Image>());

			Text lblNumber = go.GetComponentInChildren<Text>();
			if (lblNumber != null)
			{
				lblNumber.text = (i + 1).ToString();
			}
		}

		this.SetPage();
	}

	private void ResetDotPage()
	{
		if (m_DotList == null) return;

		for (int i = 0; i < m_DotList.Count; i++)
		{
			m_DotList[i].color = Color.white;
		}
	}

	private void SetPage()
	{
		if (m_DotList == null) return;

		ResetDotPage();
		m_DotList[m_PageIndex - 1].color = Color.green;

		if (txtPage != null)
		{
			txtPage.text = string.Format("{0}/{1}", m_PageIndex, m_TotalPage);
		}
	}

	private void CreateItemGrid(int offset, bool isFirst, bool isReset = false)
	{
		if (m_NewItemList == null) return;

		this.SetPage();

		if (isReset)
		{
			for (int i = 0; i < m_NewItemList.Count; i++)
			{
				DestroyImmediate(m_NewItemList[i].gameObject);
			}
		}

		m_NewItemList.Clear();
		int index = 0;//创建索引
		for (int i = 0; i < RowCount; i++)
		{
			for (int j = 0; j < ColumnCount; j++)
			{
				if (m_TotalCount == PageSize * (m_PageIndex - 1) + index) return;

				GameObject go = null;
				if (isFirst || m_Queue.Count == 0)
				{
					go = GameUtil.AddChild(itemPanel, itemPrefab); //创建
				}
				else
				{
					go = m_Queue.Dequeue();
					go.transform.parent = itemPanel;
				}

				go.name = "item " + (index < 10 ? "0" + index : index.ToString());
				go.transform.localPosition = new Vector3(j * ItemWidth + offset, i * -ItemWidth, 0);

				Button btn = go.GetComponent<Button>();
				if (btn != null)
				{
					btn.onClick.AddListener(() =>
					{
						Debug.Log("点击了" + go.name);
					});
				}

				//Debug.Log("OnItemCreate" + index);
				m_NewItemList.Add(go.transform);

				if (OnItemCreate != null)
				{
					OnItemCreate(PageSize * (m_PageIndex - 1) + index, go);
				}
				index++;
			}
		}
	}

	public void OnPageButtonCLick(string name)
	{
		if (m_IsPlaying) return;

		int offset = 0;
		switch (name)
		{
			case "Prev":
				if (m_PageIndex == 1) return;
				//Debug.Log("上一页");
				m_PageIndex--;
				offset = -ItemWidth * ColumnCount;
				break;
			case "Next":
				if (m_PageIndex == m_TotalPage) return;
				//Debug.Log("下一页");
				m_PageIndex++;
				offset = ItemWidth * ColumnCount;
				break;
		}
		m_IsPlaying = true;

		m_OldItemList.AddRange(m_NewItemList);

		CreateItemGrid(offset, false);
		int num = offset / (ItemWidth * ColumnCount);
		for (int i = m_OldItemList.Count - 1; i >= 0; i--)
		{
			GameObject item = m_OldItemList[i].gameObject;
			Tweener tween = item.transform.DOLocalMoveX(-offset, 0.3f);
			tween.SetRelative();
			tween.SetEase(Ease.InOutCubic);

			float delay = (i % ColumnCount) * num + (1 - num) * 2f;
			//乘以0.02f就是减少每一排的切换间隔
			tween.SetDelay(delay * 0.02f);
		}
		for (int i = m_NewItemList.Count - 1; i >= 0; i--)
		{
			GameObject item = m_NewItemList[i].gameObject;
			Tweener tween = item.transform.DOLocalMoveX(-offset, 0.3f);
			tween.SetRelative();
			tween.SetEase(Ease.InOutCubic);

			tween.OnComplete(OnTweenComplete);

			float delay = (i % ColumnCount) * num + (1 - num) * 2f;
			tween.SetDelay((delay + 5) * 0.02f);
		}
	}

    private int index = 0;
    private void OnTweenComplete()
    {
        index++;
        if (index == PageSize)
        {
            while (m_OldItemList.Count > 0)
            {
                m_OldItemList[0].gameObject.transform.SetParent(this.itemPool);
                m_Queue.Enqueue(m_OldItemList[0].gameObject);
                Button btn = m_OldItemList[0].gameObject.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                }
                m_OldItemList.RemoveAt(0);
            }
            m_OldItemList.Clear();
            index = 0;
            m_IsPlaying = false;
        }
    }
}
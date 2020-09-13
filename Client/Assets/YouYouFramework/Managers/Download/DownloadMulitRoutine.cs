using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// 下载多文件器
/// </summary>
public class DownloadMulitRoutine : IDisposable
{
	public DownloadMulitRoutine()
	{
		m_DownloadMulitCurrSizeDic = new Dictionary<string, ulong>();
		m_DownloadRoutineList = new LinkedList<DownloadRoutine>();
		m_NeedDownloadList = new LinkedList<string>();
	}
	public void Dispose()
	{
		m_DownloadMulitCurrSizeDic.Clear();
		m_DownloadRoutineList.Clear();
		m_NeedDownloadList.Clear();
	}
	internal void OnUpdate()
	{
		var curr = m_DownloadRoutineList.First;
		while (curr != null)
		{
			curr.Value.OnUpdate();
			curr = curr.Next;
		}
	}

	/// <summary>
	/// 下载器链表
	/// </summary>
	private LinkedList<DownloadRoutine> m_DownloadRoutineList;
	/// <summary>
	/// 需要下载的文件链表
	/// </summary>
	private LinkedList<string> m_NeedDownloadList;

	#region 下载多个文件
	/// <summary>
	/// 多个文件下载中委托
	/// </summary>
	private BaseAction<int, int, ulong, ulong> m_OnDownloadMulitUpdate;
	/// <summary>
	/// 多个文件下载完毕委托
	/// </summary>
	private BaseAction<DownloadMulitRoutine> m_OnDownloadMulitComplete;

	/// <summary>
	/// 多个文件下载_需要下载的数量
	/// </summary>
	private int m_DownloadMulitNeedCount = 0;
	/// <summary>
	/// 多个文件下载_当前下载的数量
	/// </summary>
	private int m_DownloadMulitCurrCount = 0;
	/// <summary>
	/// 多个文件下载总大小(字节)
	/// </summary>
	private ulong m_DownloadMulitTotalSize = 0;
	/// <summary>
	/// 多个文件下载当前大小(字节)
	/// </summary>
	private ulong m_DownloadMulitCurrSize = 0;
	/// <summary>
	/// 多个文件下载 当前大小
	/// </summary>
	private Dictionary<string, ulong> m_DownloadMulitCurrSizeDic;

	/// <summary>
	/// 下载多个文件
	/// </summary>
	/// <param name="lstUrl"></param>
	/// <param name="onDownloadMulitUpdate"></param>
	/// <param name="onDownloadMulitComplete"></param>
	internal void BeginDownloadMulit(LinkedList<string> lstUrl, BaseAction<int, int, ulong, ulong> onDownloadMulitUpdate, BaseAction<DownloadMulitRoutine> onDownloadMulitComplete)
	{
		if (lstUrl.Count < 1)
		{
			onDownloadMulitComplete?.Invoke(this);
			return;
		}
		m_OnDownloadMulitUpdate = onDownloadMulitUpdate;
		m_OnDownloadMulitComplete = onDownloadMulitComplete;

		m_NeedDownloadList.Clear();
		m_DownloadMulitCurrSizeDic.Clear();

		m_DownloadMulitNeedCount = 0;
		m_DownloadMulitCurrCount = 0;

		m_DownloadMulitTotalSize = 0;
		m_DownloadMulitCurrSize = 0;

		//1.把需要下载的加入下载队列
		for (LinkedListNode<string> item = lstUrl.First; item != null; item = item.Next)
		{
			string url = item.Value;
			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
			if (entity != null)
			{
				m_DownloadMulitTotalSize += entity.Size;
				m_DownloadMulitNeedCount++;
				m_NeedDownloadList.AddLast(url);
				m_DownloadMulitCurrSizeDic[url] = 0;
			}
			else
			{
				GameEntry.LogError("无效资源包=>" + url);
			}
		}

		//下载器数量
		int routineCount = Mathf.Min(GameEntry.Download.DownloadRoutineCount, m_DownloadMulitNeedCount);
		for (int i = 0; i < routineCount; i++)
		{
			DownloadRoutine routine = GameEntry.Pool.DequeueClassObject<DownloadRoutine>();

			string url = m_NeedDownloadList.First.Value;

			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);

			routine.BeginDownload(url, entity, OnDownloadMulitUpdate, OnDownloadMulitComplete);
			m_DownloadRoutineList.AddLast(routine);

			m_NeedDownloadList.RemoveFirst();
		}
	}
	private void OnDownloadMulitUpdate(string url, ulong currDownSize, float progress)
	{
		m_DownloadMulitCurrSizeDic[url] = currDownSize;

		ulong currSize = 0;
		var enumerator = m_DownloadMulitCurrSizeDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			currSize += enumerator.Current.Value;
		}

		m_DownloadMulitCurrSize = currSize;

		if (m_DownloadMulitCurrSize > m_DownloadMulitTotalSize) m_DownloadMulitCurrSize = m_DownloadMulitTotalSize;

		if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);
	}
	private void OnDownloadMulitComplete(string fileUrl, DownloadRoutine routine)
	{
		//检查队列中是否有要下载的数量
		if (m_NeedDownloadList.Count > 0)
		{
			//让下载器继续工作
			string url = m_NeedDownloadList.First.Value;

			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
			routine.BeginDownload(url, entity, OnDownloadMulitUpdate, OnDownloadMulitComplete);

			m_NeedDownloadList.RemoveFirst();
		}
		else
		{
			//下载器回池
			m_DownloadRoutineList.Remove(routine);
			GameEntry.Pool.EnqueueClassObject(routine);
		}

		m_DownloadMulitCurrCount++;

		if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

		if (m_DownloadMulitCurrCount == m_DownloadMulitNeedCount)
		{
			m_DownloadMulitCurrSize = m_DownloadMulitTotalSize;
			if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

			if (m_OnDownloadMulitComplete != null) m_OnDownloadMulitComplete(this);
			//Debug.LogError("所有资源下载完毕!!!");
		}
	}
	#endregion
}

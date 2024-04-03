using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouMain
{
    /// <summary>
    /// 下载多文件器
    /// </summary>
    public class DownloadMulitRoutine
    {
        public DownloadMulitRoutine()
        {
            m_DownloadMulitCurrSizeDic = new Dictionary<string, ulong>();
            m_DownloadRoutineList = new LinkedList<DownloadRoutine>();
            m_NeedDownloadList = new LinkedList<string>();
        }
        public static DownloadMulitRoutine Create()
        {
            return new DownloadMulitRoutine();
            //return MainEntry.ClassObjectPool.Dequeue<DownloadMulitRoutine>();
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
        private Action<int, int, ulong, ulong> m_OnDownloadMulitUpdate;
        /// <summary>
        /// 多个文件下载完毕委托
        /// </summary>
        private Action<DownloadMulitRoutine> m_OnDownloadMulitComplete;

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
        internal void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate, Action<DownloadMulitRoutine> onDownloadMulitComplete)
        {
            if (lstUrl.Count < 1)
            {
                //MainEntry.ClassObjectPool.Enqueue(this);
                onDownloadMulitComplete?.Invoke(this);
                return;
            }
            m_OnDownloadMulitUpdate = onDownloadMulitUpdate;
            m_OnDownloadMulitComplete = onDownloadMulitComplete;

            //1.把需要下载的加入下载队列
            for (LinkedListNode<string> item = lstUrl.First; item != null; item = item.Next)
            {
                string url = item.Value;
                VersionFileEntity entity = VersionCDNModel.Instance.GetVersionFileEntity(url);
                if (entity != null)
                {
                    m_DownloadMulitTotalSize += entity.Size;
                    m_DownloadMulitNeedCount++;
                    m_NeedDownloadList.AddLast(url);
                    m_DownloadMulitCurrSizeDic[url] = 0;
                }
                else
                {
                    MainEntry.LogError("无效资源包=>" + url);
                }
            }

            //下载器数量
            int routineCount = Mathf.Min(MainEntry.ParamsSettings.DownloadRoutineCount, m_DownloadMulitNeedCount);
            for (int i = 0; i < routineCount; i++)
            {
                DownloadRoutine routine = DownloadRoutine.Create();
                m_DownloadRoutineList.AddLast(routine);

                string url = m_NeedDownloadList.First.Value;
                VersionFileEntity entity = VersionCDNModel.Instance.GetVersionFileEntity(url);
                routine.BeginDownload(url, entity, OnDownloadUpdateOne, OnDownloadCompleteOne);

                m_NeedDownloadList.RemoveFirst();
            }
        }
        /// <summary>
        /// 单个文件下载进度更新
        /// </summary>
        private void OnDownloadUpdateOne(string url, ulong currDownSize, float progress)
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

            m_OnDownloadMulitUpdate?.Invoke(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);
        }
        /// <summary>
        /// 单个文件下载完毕
        /// </summary>
        private void OnDownloadCompleteOne(string fileUrl, DownloadRoutine routine)
        {
            //检查队列中是否有要下载的数量
            if (m_NeedDownloadList.Count > 0)
            {
                //复用原有的下载器，下载新文件
                string url = m_NeedDownloadList.First.Value;
                VersionFileEntity entity = VersionCDNModel.Instance.GetVersionFileEntity(url);
                routine.BeginDownload(url, entity, OnDownloadUpdateOne, OnDownloadCompleteOne);

                m_NeedDownloadList.RemoveFirst();
            }

            m_DownloadMulitCurrCount++;

            m_OnDownloadMulitUpdate?.Invoke(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

            if (m_DownloadMulitCurrCount == m_DownloadMulitNeedCount)
            {
                m_DownloadMulitCurrSize = m_DownloadMulitTotalSize;
                m_OnDownloadMulitUpdate?.Invoke(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

                //MainEntry.ClassObjectPool.Enqueue(this);
                m_OnDownloadMulitComplete?.Invoke(this);
                //Debug.LogError("所有资源下载完毕!!!");
            }
        }
        #endregion
    }
}
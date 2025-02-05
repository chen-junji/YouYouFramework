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
        public static DownloadMulitRoutine Create()
        {
            return new DownloadMulitRoutine();
            //return MainEntry.ClassObjectPool.Dequeue<DownloadMulitRoutine>();
        }

        #region 下载多个文件
        /// <summary>
        /// 多个文件下载中委托
        /// </summary>
        private Action<int, int, ulong, ulong> m_OnDownloadMulitUpdate;
        /// <summary>
        /// 多个文件下载完毕委托
        /// </summary>
        private Action<bool> m_OnDownloadMulitComplete;

        /// <summary>
        /// 多个文件下载_需要下载的数量
        /// </summary>
        private int m_DownloadMulitTotalCount = 0;
        /// <summary>
        /// 多个文件下载_当前下载的数量
        /// </summary>
        private int m_DownloadMulitCurrCount = 0;
        /// <summary>
        /// 当前下载成功数量
        /// </summary>
        private int downloadMulitSuccessCount = 0;
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
        private Dictionary<string, ulong> m_DownloadMulitCurrSizeDic = new();

        /// <summary>
        /// 下载多个文件
        /// </summary>
        internal void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate, Action<bool> onDownloadMulitComplete)
        {
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
                    m_DownloadMulitTotalCount++;
                    m_DownloadMulitCurrSizeDic[url] = 0;

                    MainEntry.Download.BeginDownloadSingle(url, OnDownloadUpdateOne, OnDownloadCompleteOne);
                }
                else
                {
                    MainEntry.LogError("无效资源包=>" + url);
                }
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

            m_OnDownloadMulitUpdate?.Invoke(m_DownloadMulitCurrCount, m_DownloadMulitTotalCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);
        }
        /// <summary>
        /// 单个文件下载完毕
        /// </summary>
        private void OnDownloadCompleteOne(bool success)
        {
            if (success)
            {
                downloadMulitSuccessCount++;
            }
            m_DownloadMulitCurrCount++;

            if (m_DownloadMulitCurrCount == m_DownloadMulitTotalCount)
            {
                //MainEntry.ClassObjectPool.Enqueue(this);
                m_OnDownloadMulitComplete?.Invoke(downloadMulitSuccessCount == m_DownloadMulitTotalCount);
                //Debug.LogError("所有资源下载完毕!!!");
            }
        }
        #endregion
    }
}
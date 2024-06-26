using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouMain
{
    /// <summary>
    /// 下载管理器
    /// </summary>
    public class DownloadManager
    {
        /// <summary>
        /// 下载单文件器链表
        /// </summary>
        private LinkedList<DownloadRoutine> m_DownloadSingleRoutineList;

        /// <summary>
        /// 下载多文件器链表
        /// </summary>
        private LinkedList<DownloadMulitRoutine> m_DownloadMulitRoutineList;

        internal DownloadManager()
        {
            m_DownloadSingleRoutineList = new LinkedList<DownloadRoutine>();
            m_DownloadMulitRoutineList = new LinkedList<DownloadMulitRoutine>();
        }
        internal void OnUpdate()
        {
            //调用下载单文件器的OnUpdate()
            var singleRoutine = m_DownloadSingleRoutineList.First;
            while (singleRoutine != null)
            {
                singleRoutine.Value.OnUpdate();
                singleRoutine = singleRoutine.Next;
            }

            //调用下载多文件器的OnUpdate()
            var mulitRoutine = m_DownloadMulitRoutineList.First;
            while (mulitRoutine != null)
            {
                mulitRoutine.Value.OnUpdate();
                mulitRoutine = mulitRoutine.Next;
            }
        }

        /// <summary>
        /// 下载单个文件
        /// </summary>
        public void BeginDownloadSingle(string url, Action<string, ulong, float> onUpdate = null, Action<string> onComplete = null)
        {
            VersionFileEntity entity = VersionCDNModel.Instance.GetVersionFileEntity(url);
            if (entity == null)
            {
                MainEntry.LogError("无效资源包=>" + url);
                return;
            }

            DownloadRoutine routine = DownloadRoutine.Create();
            m_DownloadSingleRoutineList.AddLast(routine);
            routine.BeginDownload(url, entity, onUpdate, onComplete: (string fileUrl, DownloadRoutine r) =>
            {
                m_DownloadSingleRoutineList.Remove(routine);
                if (onComplete != null) onComplete(fileUrl);
            });
        }

        /// <summary>
        /// 下载多个文件
        /// </summary>
        public void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate = null, Action onDownloadMulitComplete = null)
        {
            DownloadMulitRoutine mulitRoutine = DownloadMulitRoutine.Create();
            mulitRoutine.BeginDownloadMulit(lstUrl, onDownloadMulitUpdate, (DownloadMulitRoutine r) =>
            {
                m_DownloadMulitRoutineList.Remove(r);
                onDownloadMulitComplete?.Invoke();
            });
            m_DownloadMulitRoutineList.AddLast(mulitRoutine);
        }

    }
}
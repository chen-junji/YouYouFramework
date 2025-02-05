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
        public class DownlaodTask
        {
            public string donwloadUrl;
            public VersionFileEntity downloadEntity;
            public Action<string, ulong, float> downloadUpdate;
            public Action<bool> downloadComplete;
        }

        /// <summary>
        /// 正在下载的下载器
        /// </summary>
        private LinkedList<DownloadRoutine> m_DownloadSingleRoutineList = new();

        /// <summary>
        /// 空闲的下载器 缓存池
        /// </summary>
        private LinkedList<DownloadRoutine> m_DownloadRoutineList = new();

        /// <summary>
        /// 需要下载的文件链表
        /// </summary>
        private LinkedList<DownlaodTask> m_NeedDownloadList = new();

        internal DownloadManager()
        {
            //下载器数量
            for (int i = 0; i < MainEntry.Instance.DownloadRoutineCount; i++)
            {
                DownloadRoutine routine = DownloadRoutine.Create();
                m_DownloadRoutineList.AddLast(routine);
            }

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
        }

        /// <summary>
        /// 下载单个文件
        /// </summary>
        public void BeginDownloadSingle(string url, Action<string, ulong, float> onUpdate = null, Action<bool> onComplete = null)
        {
            VersionFileEntity entity = VersionCDNModel.Instance.GetVersionFileEntity(url);
            if (entity == null)
            {
                MainEntry.LogError("无效资源包=>" + url);
                return;
            }

            m_NeedDownloadList.AddLast(new DownlaodTask { donwloadUrl = url, downloadEntity = entity, downloadUpdate = onUpdate, downloadComplete = onComplete });
            BeginDownloadSingle();
        }
        private void BeginDownloadSingle()
        {
            //检查队列中是否有要下载的数量
            if (m_NeedDownloadList.Count > 0 && m_DownloadRoutineList.Count > 0)
            {
                //取池
                DownloadRoutine routine = m_DownloadRoutineList.First.Value;
                m_DownloadRoutineList.RemoveFirst();
                m_DownloadSingleRoutineList.AddLast(routine);

                DownlaodTask downlaodTask = m_NeedDownloadList.First.Value;
                m_NeedDownloadList.RemoveFirst();
                routine.BeginDownload(downlaodTask.donwloadUrl, downlaodTask.downloadEntity, downlaodTask.downloadUpdate, (success) =>
                {
                    downlaodTask.downloadComplete?.Invoke(success);
                    //回池
                    m_DownloadSingleRoutineList.Remove(routine);
                    m_DownloadRoutineList.AddLast(routine);
                    BeginDownloadSingle();
                });
            }

        }

        /// <summary>
        /// 下载多个文件
        /// </summary>
        public void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate = null, Action<bool> onDownloadMulitComplete = null)
        {
            if (lstUrl.Count < 1)
            {
                onDownloadMulitComplete?.Invoke(false);
                return;
            }
            DownloadMulitRoutine mulitRoutine = DownloadMulitRoutine.Create();
            mulitRoutine.BeginDownloadMulit(lstUrl, onDownloadMulitUpdate, onDownloadMulitComplete);
        }

    }
}
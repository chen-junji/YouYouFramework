using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 下载管理器
    /// </summary>
    public class DownloadManager
    {
        public int FlushSize { get; private set; }

        public int DownloadRoutineCount { get; private set; }

        /// <summary>
        /// 连接失败后的重试次数
        /// </summary>
        public int Retry { get; private set; }

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
        internal void Dispose()
        {
            m_DownloadSingleRoutineList.Clear();

            //调用下载多文件器的Dispose()
            var mulitRoutine = m_DownloadMulitRoutineList.First;
            while (mulitRoutine != null)
            {
                mulitRoutine.Value.Dispose();
                mulitRoutine = mulitRoutine.Next;
            }
            m_DownloadMulitRoutineList.Clear();
        }
        /// <summary>
        /// 更新
        /// </summary>
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
        internal void Init()
        {
            Retry = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_Retry, GameEntry.CurrDeviceGrade);
            DownloadRoutineCount = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_RoutineCount, GameEntry.CurrDeviceGrade);
            FlushSize = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_FlushSize, GameEntry.CurrDeviceGrade);
        }

        /// <summary>
        /// 下载单个文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onUpdate"></param>
        public void BeginDownloadSingle(string url, Action<string, ulong, float> onUpdate = null, Action<string> onComplete = null)
        {
            AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
            if (entity == null)
            {
                GameEntry.LogError("无效资源包=>" + url);
                return;
            }

            DownloadRoutine routine = GameEntry.Pool.DequeueClassObject<DownloadRoutine>();
            routine.BeginDownload(url, entity, onUpdate, onComplete: (string fileUrl, DownloadRoutine r) =>
            {
                m_DownloadSingleRoutineList.Remove(routine);
                GameEntry.Pool.EnqueueClassObject(routine);
                if (onComplete != null) onComplete(fileUrl);
            });
            m_DownloadSingleRoutineList.AddLast(routine);
        }

        /// <summary>
        /// 下载多个文件
        /// </summary>
        /// <param name="lstUrl"></param>
        /// <param name="onDownloadMulitUpdate"></param>
        /// <param name="onDownloadMulitComplete"></param>
        public void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate = null, Action onDownloadMulitComplete = null)
        {
            DownloadMulitRoutine mulitRoutine = GameEntry.Pool.DequeueClassObject<DownloadMulitRoutine>();
            mulitRoutine.BeginDownloadMulit(lstUrl, onDownloadMulitUpdate, (DownloadMulitRoutine r) =>
            {
                m_DownloadMulitRoutineList.Remove(r);
                GameEntry.Pool.EnqueueClassObject(r);
                onDownloadMulitComplete?.Invoke();
            });
            m_DownloadMulitRoutineList.AddLast(mulitRoutine);
        }

    }
}

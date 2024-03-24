using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    /// <summary>
    /// 下载器
    /// </summary>
    public class DownloadRoutine
    {
        /// <summary>
        /// Web请求
        /// </summary>
        private UnityWebRequest m_UnityWebRequest = null;

        /// <summary>
        /// 文件流
        /// </summary>
        private FileStream m_FileStream;

        /// <summary>
        /// 当前等待写入磁盘的大小
        /// </summary>
        private int m_CurrWaitFlushSize = 0;
        /// <summary>
        /// 上次写入的大小
        /// </summary>
        private int m_PrevWriteSize = 0;
        /// <summary>
        /// 文件总大小
        /// </summary>
        private ulong m_TotalSize;
        /// <summary>
        /// 当前下载的大小
        /// </summary>
        private ulong m_CurrDownloaderSize = 0;
        /// <summary>
        /// 起始位置
        /// </summary>
        private uint m_BeginPos = 0;

        /// <summary>
        /// 当前文件路径
        /// </summary>
        public string m_CurrFileUrl;

        /// <summary>
        /// 当前的资源包信息
        /// </summary>
        private VersionFileEntity m_CurrAssetBundleInfo;

        /// <summary>
        /// 当前重试次数
        /// </summary>
        private int m_CurrRetry;

        /// <summary>
        /// 下载的本地文件路径
        /// </summary>
        private string m_DownloadLocalFilePath;

        /// <summary>
        /// 下载中委托
        /// </summary>
        private Action<string, ulong, float> m_OnUpdate;
        /// <summary>
        /// 下载完毕委托
        /// </summary>
        private Action<string, DownloadRoutine> m_OnComplete;

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onUpdate"></param>
        /// <param name="onComplete"></param>
        public void BeginDownload(string url, VersionFileEntity assetBundleInfo, Action<string, ulong, float> onUpdate, Action<string, DownloadRoutine> onComplete)
        {
            m_CurrFileUrl = url;
            m_CurrAssetBundleInfo = assetBundleInfo;
            m_OnUpdate = onUpdate;
            m_OnComplete = onComplete;
            m_CurrRetry = 0;

            m_DownloadLocalFilePath = string.Format("{0}/{1}", Application.persistentDataPath, m_CurrFileUrl);

            //如果本地已有目标文件, 则删除
            if (File.Exists(m_DownloadLocalFilePath)) File.Delete(m_DownloadLocalFilePath);

            //下载新版本的资源
            m_DownloadLocalFilePath = m_DownloadLocalFilePath + ".temp";
            DownloadInner();
        }
        /// <summary>
        /// 内部下载
        /// </summary>
        private void DownloadInner()
        {
            if (File.Exists(m_DownloadLocalFilePath))
            {
                if (PlayerPrefs.HasKey(m_CurrFileUrl))
                {
                    if (!PlayerPrefs.GetString(m_CurrFileUrl).Equals(m_CurrAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //根据MD5判断 跟CDN上的MD5不一致则删除原文件重新下载
                        File.Delete(m_DownloadLocalFilePath);
                        BeginDownload();
                    }
                    else
                    {
                        //断点续传_继续下载
                        m_FileStream = File.OpenWrite(m_DownloadLocalFilePath);
                        m_FileStream.Seek(0, SeekOrigin.End);
                        m_BeginPos = (uint)m_FileStream.Length;
                        Download(string.Format("{0}{1}", SystemModel.Instance.CurrChannelConfig.RealSourceUrl, m_CurrFileUrl), m_BeginPos);
                    }
                }
                else
                {
                    //本地没有MD5记录,重新下载
                    File.Delete(m_DownloadLocalFilePath);
                    BeginDownload();
                }
            }
            else
            {
                BeginDownload();
            }
        }

        /// <summary>
        /// 从零下载
        /// </summary>
        private void BeginDownload()
        {
            string directory = Path.GetDirectoryName(m_DownloadLocalFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            m_FileStream = new FileStream(m_DownloadLocalFilePath, FileMode.Create, FileAccess.Write);

            PlayerPrefs.SetString(m_CurrFileUrl, m_CurrAssetBundleInfo.MD5);
            Download(string.Format("{0}{1}", SystemModel.Instance.CurrChannelConfig.RealSourceUrl, m_CurrFileUrl));
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url"></param>
        private void Download(string url)
        {
            m_UnityWebRequest = UnityWebRequest.Get(url);
            m_UnityWebRequest.SendWebRequest();
        }
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="beginPos"></param>
        private void Download(string url, uint beginPos)
        {
            m_UnityWebRequest = UnityWebRequest.Get(url);
            m_UnityWebRequest.SetRequestHeader("Range", string.Format("bytes={0}-", beginPos.ToString()));
            m_UnityWebRequest.SendWebRequest();
        }

        public static DownloadRoutine Create()
        {
            return MainEntry.ClassObjectPool.Dequeue<DownloadRoutine>();
        }
        public void Reset()
        {
            if (m_UnityWebRequest != null)
            {
                m_UnityWebRequest.Dispose();
                m_UnityWebRequest = null;
            }

            if (m_FileStream != null)
            {
                m_FileStream.Close();
                m_FileStream.Dispose();
                m_FileStream = null;
            }

            m_PrevWriteSize = 0;
            m_TotalSize = 0;
            m_CurrDownloaderSize = 0;
            m_CurrWaitFlushSize = 0;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            if (m_UnityWebRequest == null) return;

            //计算当前下载文件总大小
            if (m_TotalSize == 0)
            {
                ulong.TryParse(m_UnityWebRequest.GetResponseHeader("Content-Length"), out m_TotalSize);
            }

            //计算下载进度
            if (!m_UnityWebRequest.isDone)
            {
                if (m_CurrDownloaderSize < m_UnityWebRequest.downloadedBytes)//这里是为了判断当前是否处于下载未完成
                {
                    m_CurrDownloaderSize = m_UnityWebRequest.downloadedBytes;
                    //YouYou.GameEntry.LogError(string.Format("下载进度{0}%", (int)(m_CurrDownloaderSize / (float)m_TotalSize) * 100));

                    Sava(m_UnityWebRequest.downloadHandler.data);

                    if (m_OnUpdate != null) m_OnUpdate(m_CurrFileUrl, m_CurrDownloaderSize, m_CurrDownloaderSize / (float)m_TotalSize);
                }
                return;
            }
            switch (m_UnityWebRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    m_CurrRetry++;
                    if (m_CurrRetry <= MainEntry.ParamsSettings.DownloadRetry)
                    {
                        MainEntry.Log(MainEntry.LogCategory.Assets, "下载文件URL {0} 出错, 正在进行重试, 当前重试次数{1}", m_UnityWebRequest.url, m_CurrRetry);
                        Reset();
                        DownloadInner();
                        return;
                    }
                    MainEntry.LogError(MainEntry.LogCategory.Assets, "下载失败, URL {0} Error= {1}", m_UnityWebRequest.url, m_UnityWebRequest.error);
                    Reset();
                    MainEntry.ClassObjectPool.Enqueue(this);
                    return;
            }

            //下载完毕
            MainEntry.Log(MainEntry.LogCategory.Assets, "下载完毕=>" + m_UnityWebRequest.url);
            m_CurrDownloaderSize = m_UnityWebRequest.downloadedBytes;
            Sava(m_UnityWebRequest.downloadHandler.data, true);

            if (m_OnUpdate != null) m_OnUpdate(m_CurrFileUrl, m_CurrDownloaderSize, m_CurrDownloaderSize / (float)m_TotalSize);

            Reset();

            File.Move(m_DownloadLocalFilePath, m_DownloadLocalFilePath.Replace(".temp", ""));
            m_DownloadLocalFilePath = null;

            if (PlayerPrefs.HasKey(m_CurrFileUrl)) PlayerPrefs.DeleteKey(m_CurrFileUrl);

            //更新可写区的版本信息
            MainEntry.Assets.VersionFile.SaveVersion(m_CurrAssetBundleInfo);

            MainEntry.ClassObjectPool.Enqueue(this);
            m_OnComplete?.Invoke(m_CurrFileUrl, this);
        }

        /// <summary>
        /// 保存字节
        /// </summary>
        /// <param name="buffer"></param>
        private void Sava(byte[] buffer, bool downloadComplete = false)
        {
            if (buffer == null) return;

            int len = buffer.Length;
            int count = len - m_PrevWriteSize;
            m_FileStream.Write(buffer, m_PrevWriteSize, count);
            m_PrevWriteSize = len;

            m_CurrWaitFlushSize += count;
            if (m_CurrWaitFlushSize >= MainEntry.ParamsSettings.DownloadFlushSize || downloadComplete)
            {
                //YouYou.GameEntry.LogError("写入磁盘" + m_CurrFileUrl);
                m_CurrWaitFlushSize = 0;
                m_FileStream.Flush();
            }
        }
    }
}
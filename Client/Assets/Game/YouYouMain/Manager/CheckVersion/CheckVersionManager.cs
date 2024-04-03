using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YouYouMain
{
    public class CheckVersionManager
    {
        /// <summary>
        /// 版本文件 管理器
        /// </summary>
        public VersionFileManager VersionFile { get; private set; }

        /// <summary>
        /// 需要下载的资源包列表
        /// </summary>
        private LinkedList<string> m_NeedDownloadList;

        /// <summary>
        /// 检查版本更新下载时候的参数
        /// </summary>
        private BaseParams m_DownloadingParams;

        public event Action CheckVersionBeginDownload;
        public event Action<BaseParams> CheckVersionDownloadUpdate;
        public event Action CheckVersionDownloadComplete;

        public Action CheckVersionComplete;


        public CheckVersionManager()
        {
            VersionFile = new VersionFileManager();

            m_NeedDownloadList = new LinkedList<string>();
        }

        internal void Init()
        {
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        public void CheckVersionChange()
        {
            MainEntry.Log("检查更新=>CheckVersionChange(), 版本号=>{0}", VersionFile.LocalAssetsVersion);

            if (VersionFile.GetVersionFileExists())
            {
                if (!string.IsNullOrEmpty(VersionFile.LocalAssetsVersion) && VersionFile.LocalAssetsVersion.Equals(VersionFile.CDNVersion))
                {
                    MainEntry.Log("可写区版本号和CDN版本号一致 进入预加载流程");
                    CheckVersionComplete?.Invoke();
                }
                else
                {
                    MainEntry.Log("可写区版本号和CDN版本号不一致 开始检查更新");
                    BeginCheckVersionChange();
                }
            }
            else
            {
                //下载初始资源
                DownloadInitResources();
            }
        }

        /// <summary>
        /// 下载初始资源
        /// </summary>
        private void DownloadInitResources()
        {
            CheckVersionBeginDownload?.Invoke();
            m_DownloadingParams = BaseParams.Create();

            m_NeedDownloadList.Clear();

            var enumerator = VersionFile.m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                VersionFileEntity entity = enumerator.Current.Value;
                if (entity.IsFirstData)
                {
                    m_NeedDownloadList.AddLast(entity.AssetBundleName);
                }
            }

            //如果没有初始资源 直接检查更新
            if (m_NeedDownloadList.Count == 0)
            {
                BeginCheckVersionChange();
            }
            else
            {
                MainEntry.Log("下载初始资源,文件数量==>>" + m_NeedDownloadList.Count);
                MainEntry.Download.BeginDownloadMulit(m_NeedDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
            }
        }

        /// <summary>
        /// 开始检查更新
        /// </summary>
        private void BeginCheckVersionChange()
        {
            m_DownloadingParams = BaseParams.Create();

            //需要删除的文件
            LinkedList<string> deleteList = new LinkedList<string>();

            //需要下载的文件
            LinkedList<string> needDownloadList = new LinkedList<string>();

            //找出需要删除的文件
            var enumerator = VersionFile.m_LocalAssetsVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string assetBundleName = enumerator.Current.Key;

                VersionFileEntity cdnAssetBundleInfo = null;
                if (VersionFile.m_CDNVersionDic.TryGetValue(assetBundleName, out cdnAssetBundleInfo))
                {
                    //可写区有 CDN也有
                    if (!cdnAssetBundleInfo.MD5.Equals(enumerator.Current.Value.MD5, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //如果MD5不一致 加入下载链表
                        needDownloadList.AddLast(assetBundleName);
                    }
                }
                else
                {
                    //可写区有 CDN上没有 加入删除链表
                    deleteList.AddLast(assetBundleName);
                }
            }

            //删除需要删除的
            MainEntry.Log("删除旧资源=>{0}", deleteList.ToJson());
            LinkedListNode<string> currDel = deleteList.First;
            while (currDel != null)
            {
                StringBuilder sbr = StringHelper.PoolNew();
                string filePath = sbr.AppendFormatNoGC("{0}/{1}", Application.persistentDataPath, currDel.Value).ToString();
                StringHelper.PoolDel(ref sbr);

                if (File.Exists(filePath)) File.Delete(filePath);
                LinkedListNode<string> next = currDel.Next;
                deleteList.Remove(currDel);
                currDel = next;
            }

            //检查需要下载的
            enumerator = VersionFile.m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                VersionFileEntity cdnAssetBundleInfo = enumerator.Current.Value;
                if (cdnAssetBundleInfo.IsFirstData)//检查初始资源
                {
                    if (!VersionFile.m_LocalAssetsVersionDic.ContainsKey(cdnAssetBundleInfo.AssetBundleName))
                    {
                        //如果可写区没有 加入下载链表
                        needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                    }
                }
            }

            CheckVersionBeginDownload?.Invoke();

            //进行下载
            MainEntry.Log("下载更新资源,文件数量==>" + needDownloadList.Count + "==>" + needDownloadList.ToJson());
            MainEntry.Download.BeginDownloadMulit(needDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
        /// <summary>
        /// 下载进行中
        /// </summary>
        private void OnDownloadMulitUpdate(int t1, int t2, ulong t3, ulong t4)
        {
            m_DownloadingParams.IntParam1 = t1;
            m_DownloadingParams.IntParam2 = t2;

            m_DownloadingParams.ULongParam1 = t3;
            m_DownloadingParams.ULongParam2 = t4;

            CheckVersionDownloadUpdate?.Invoke(m_DownloadingParams);
        }
        /// <summary>
        /// 下载完毕
        /// </summary>
        private void OnDownloadMulitComplete()
        {
            VersionFile.SetAssetVersion(VersionFile.CDNVersion);

            CheckVersionDownloadComplete?.Invoke();
            MainEntry.ClassObjectPool.Enqueue(m_DownloadingParams);

            MainEntry.Log("检查更新下载完毕 进入预加载流程");
            CheckVersionComplete?.Invoke();
        }


        /// <summary>
        /// 单个文件检查更新(True==不需要更新)
        /// </summary>
        public bool CheckVersionChangeSingle(string assetBundleName)
        {
            if (VersionFile.m_CDNVersionDic.TryGetValue(assetBundleName, out VersionFileEntity cdnAssetBundleInfo))
            {
                if (VersionFile.m_LocalAssetsVersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleName, out VersionFileEntity LocalAssetsAssetBundleInfo))
                {
                    //可写区有 CDN也有 验证MD5
                    return cdnAssetBundleInfo.MD5.Equals(LocalAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return false;//CDN不存在
        }

    }
}
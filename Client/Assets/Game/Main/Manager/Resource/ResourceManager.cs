using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class ResourceManager
    {
        /// <summary>
        /// 可写区管理器
        /// </summary>
        public LocalAssetsManager LocalAssetsManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 需要下载的资源包列表
        /// </summary>
        private LinkedList<string> m_NeedDownloadList;

        /// <summary>
        /// 检查版本更新下载时候的参数
        /// </summary>
        private BaseParams m_DownloadingParams;

        /// <summary>
        /// 本地文件路径
        /// </summary>
        public string LocalFilePath { get; private set; }

        public ResourceManager()
        {
            LocalAssetsManager = new LocalAssetsManager();

            m_NeedDownloadList = new LinkedList<string>();
        }

        internal void Init()
        {
#if EDITORLOAD
            LocalFilePath = Application.dataPath;
#else
            LocalFilePath = Application.persistentDataPath;
#endif
        }

        #region GetAssetBundleVersionList 根据字节数组获取资源包版本信息
        /// <summary>
        /// 根据字节数组获取资源包版本信息
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public static Dictionary<string, AssetBundleInfoEntity> GetAssetBundleVersionList(byte[] buffer, ref string version)
        {
            buffer = ZlibHelper.DeCompressBytes(buffer);

            Dictionary<string, AssetBundleInfoEntity> dic = new Dictionary<string, AssetBundleInfoEntity>();

            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);

            int len = ms.ReadInt();

            for (int i = 0; i < len; i++)
            {
                if (i == 0)
                {
                    version = ms.ReadUTF8String().Trim();
                }
                else
                {
                    AssetBundleInfoEntity entity = new AssetBundleInfoEntity();
                    entity.AssetBundleName = ms.ReadUTF8String();
                    entity.MD5 = ms.ReadUTF8String();
                    entity.Size = ms.ReadULong();
                    entity.IsFirstData = ms.ReadByte() == 1;
                    entity.IsEncrypt = ms.ReadByte() == 1;

                    dic[entity.AssetBundleName] = entity;
                }
            }
            return dic;
        }
        #endregion

        #region CDN
        /// <summary>
        /// CDN资源版本号
        /// </summary>
        private string m_CDNVersion;

        /// <summary>
        /// CDN资源版本号
        /// </summary>
        public string CDNVersion { get { return m_CDNVersion; } }

        /// <summary>
        /// CDN资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_CDNVersionDic = new Dictionary<string, AssetBundleInfoEntity>();

        /// <summary>
        /// 初始化CDN的版本文件信息
        /// </summary>
        public void InitCDNVersionFile(Action onInitComplete)
        {
            StringBuilder sbr = StringHelper.PoolNew();
            string url = sbr.AppendFormatNoGC("{0}{1}", MainEntry.Data.CurrChannelConfig.RealSourceUrl, YFConstDefine.VersionFileName).ToString();
            StringHelper.PoolDel(ref sbr);

            IEnumerator UnityWebRequestGet(string url, Action<UnityWebRequest> onComplete)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    yield return request.SendWebRequest();
                    onComplete?.Invoke(request);
                }
            }
            MainEntry.Instance.StartCoroutine(UnityWebRequestGet(url, (request) =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_CDNVersionDic = GetAssetBundleVersionList(request.downloadHandler.data, ref m_CDNVersion);
                    MainEntry.Log(MainEntry.LogCategory.Resource, "OnInitCDNVersionFile");
                    if (LocalAssetsManager.GetVersionFileExists())
                    {
                        //可写区版本文件存在，加载版本文件信息
                        m_LocalAssetsVersionDic = LocalAssetsManager.GetAssetBundleVersionList(ref m_LocalAssetsVersion);
                        MainEntry.Log(MainEntry.LogCategory.Resource, "OnInitLocalVersionFile");
                    }
                    onInitComplete?.Invoke();
                }
                else
                {
                    Main.MainEntry.Log(MainEntry.LogCategory.Resource, "初始化CDN资源包信息失败，url==" + url);
                }
            }));
        }
        #endregion

        #region 可写区

        /// <summary>
        /// 可写区资源版本号
        /// </summary>
        private string m_LocalAssetsVersion;

        /// <summary>
        /// 可写区资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();

        /// <summary>
        /// 保存版本信息
        /// </summary>
        /// <param name="entity"></param>
        public void SaveVersion(AssetBundleInfoEntity entity)
        {
            m_LocalAssetsVersionDic[entity.AssetBundleName] = entity;

            //保存版本文件
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDic);
        }

        /// <summary>
        /// 保存资源版本号（用于检查版本更新完毕后 保存）
        /// </summary>
        public void SetResourceVersion()
        {
            m_LocalAssetsVersion = m_CDNVersion;
            LocalAssetsManager.SetResourceVersion(m_LocalAssetsVersion);
        }
        #endregion

        /// <summary>
        /// 获取CDN上的资源包信息(这个方法一定要能返回资源包信息)
        /// </summary>
        /// <param name="assetbundlePath"></param>
        /// <returns></returns>
        public AssetBundleInfoEntity GetAssetBundleInfo(string assetbundlePath)
        {
            AssetBundleInfoEntity entity = null;
            m_CDNVersionDic.TryGetValue(assetbundlePath, out entity);
            return entity;
        }

        #region 检查更新
        public event Action CheckVersionBeginDownload;
        public event Action<BaseParams> CheckVersionDownloadUpdate;
        public event Action CheckVersionDownloadComplete;

        public Action CheckVersionComplete;

        /// <summary>
        /// 检查更新
        /// </summary>
        public void CheckVersionChange()
        {
            MainEntry.Log(MainEntry.LogCategory.Resource, "检查更新=>CheckVersionChange(), 版本号=>{0}", m_LocalAssetsVersion);

            if (LocalAssetsManager.GetVersionFileExists())
            {
                if (!string.IsNullOrEmpty(m_LocalAssetsVersion) && m_LocalAssetsVersion.Equals(m_CDNVersion))
                {
                    MainEntry.Log(MainEntry.LogCategory.Resource, "可写区版本号和CDN版本号一致 进入预加载流程");
                    CheckVersionComplete?.Invoke();
                }
                else
                {
                    MainEntry.Log(MainEntry.LogCategory.Resource, "可写区版本号和CDN版本号不一致 开始检查更新");
                    BeginCheckVersionChange();
                }
            }
            else
            {
                //下载初始资源
                DownloadInitResources();
            }
        }

        #region DownloadInitResources 下载初始资源
        /// <summary>
        /// 下载初始资源
        /// </summary>
        private void DownloadInitResources()
        {
            CheckVersionBeginDownload?.Invoke();
            m_DownloadingParams = BaseParams.Create();

            m_NeedDownloadList.Clear();

            var enumerator = m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity entity = enumerator.Current.Value;
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
                MainEntry.Log(MainEntry.LogCategory.Resource, "下载初始资源,文件数量==>>" + m_NeedDownloadList.Count);
                MainEntry.Download.BeginDownloadMulit(m_NeedDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
            }
        }
        #endregion

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
            var enumerator = m_LocalAssetsVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string assetBundleName = enumerator.Current.Key;

                AssetBundleInfoEntity cdnAssetBundleInfo = null;
                if (m_CDNVersionDic.TryGetValue(assetBundleName, out cdnAssetBundleInfo))
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
            MainEntry.Log(MainEntry.LogCategory.Resource, "删除旧资源=>{0}", deleteList.ToJson());
            LinkedListNode<string> currDel = deleteList.First;
            while (currDel != null)
            {
                StringBuilder sbr = StringHelper.PoolNew();
                string filePath = sbr.AppendFormatNoGC("{0}/{1}", MainEntry.ResourceManager.LocalFilePath, currDel.Value).ToString();
                StringHelper.PoolDel(ref sbr);

                if (File.Exists(filePath)) File.Delete(filePath);
                LinkedListNode<string> next = currDel.Next;
                deleteList.Remove(currDel);
                currDel = next;
            }

            //检查需要下载的
            enumerator = m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity cdnAssetBundleInfo = enumerator.Current.Value;
                if (cdnAssetBundleInfo.IsFirstData)//检查初始资源
                {
                    if (!m_LocalAssetsVersionDic.ContainsKey(cdnAssetBundleInfo.AssetBundleName))
                    {
                        //如果可写区没有 加入下载链表
                        needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                    }
                }
            }

            CheckVersionBeginDownload?.Invoke();

            //进行下载
            MainEntry.Log(MainEntry.LogCategory.Resource, "下载更新资源,文件数量==>" + needDownloadList.Count + "==>" + needDownloadList.ToJson());
            MainEntry.Download.BeginDownloadMulit(needDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }

        /// <summary>
        /// 单个文件检查更新(True==不需要更新)
        /// </summary>
        public bool CheckVersionChangeSingle(string assetBundleName)
        {
            if (m_CDNVersionDic.TryGetValue(assetBundleName, out AssetBundleInfoEntity cdnAssetBundleInfo))
            {
                if (m_LocalAssetsVersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleName, out AssetBundleInfoEntity LocalAssetsAssetBundleInfo))
                {
                    //可写区有 CDN也有 验证MD5
                    return cdnAssetBundleInfo.MD5.Equals(LocalAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return false;//CDN不存在
        }
        #endregion

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
            SetResourceVersion();

            CheckVersionDownloadComplete?.Invoke();
            MainEntry.ClassObjectPool.Enqueue(m_DownloadingParams);

            MainEntry.Log(MainEntry.LogCategory.Resource, "检查更新下载完毕 进入预加载流程");
            CheckVersionComplete?.Invoke();
        }

    }
}
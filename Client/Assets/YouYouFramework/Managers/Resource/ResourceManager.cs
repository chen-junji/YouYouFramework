using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : ManagerBase
    {
        public StreamingAssetsManager StreamingAssetsManager { get; private set; }

        public LocalAssetsManager LocalAssetsManager { get; private set; }

        /// <summary>
        /// 需要下载的资源包列表
        /// </summary>
        private LinkedList<string> m_NeedDownloadList;
        /// <summary>
        /// 检查版本更新下载 用到的参数
        /// </summary>
        private BaseParams m_DownloadingParams;

        public ResourceManager()
        {
            StreamingAssetsManager = new StreamingAssetsManager();
            LocalAssetsManager = new LocalAssetsManager();
            m_NeedDownloadList = new LinkedList<string>();
            m_StreamingAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            m_CDNVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
		}

		internal override void Init()
		{

		}


		#region GetAssetBundleVersionList 根据字节数组获取资源包版本信息(解析版本文件)
		/// <summary>
		/// 根据字节数组获取资源包版本信息
		/// </summary>
		/// <param name="buffer">解析这个Buffer获取资源包版本信息</param>
		/// <param name="version">版本号</param>
		/// <returns>Key:资源包名称;Value:资源包版本信息</returns>
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
                    //版本号
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

        #region 只读区
        /// <summary>
        /// 只读区资源版本号
        /// </summary>
        private string m_StreamingAssetsVersion;

        /// <summary>
        /// 只读区资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_StreamingAssetsVersionDic;

        /// <summary>
        /// 是否存在只读区资源包信息
        /// </summary>
        private bool m_IsExistsStreamingAssetsBundleInfo;

        #region InitStreamingAssetsBundleInfo 初始化只读区资源包信息
        /// <summary>
        /// 初始化只读区资源包信息
        /// </summary>
        public void InitStreamingAssetsBundleInfo()
        {
            ReadStreamingAssetsBundle("VersionFile.bytes", (byte[] buffer) =>
            {
                if (buffer == null)
                {
                    InitCDNAssetBundleInfo();
                }
                else
                {
                    m_IsExistsStreamingAssetsBundleInfo = true;
                    m_StreamingAssetsVersionDic = GetAssetBundleVersionList(buffer, ref m_StreamingAssetsVersion);

                    GameEntry.Log(LogCategory.Resource, "读取只读区的资源包成功=>ReadStreamingAssetsBundle=>onComplete()");

                    InitCDNAssetBundleInfo();
                }
            });
        }
        #endregion

        #region ReadStreamingAssetsBundle 读取只读区的资源包
        /// <summary>
        /// 读取只读区的资源包
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="onComplete"></param>
        internal void ReadStreamingAssetsBundle(string fileUrl, Action<byte[]> onComplete)
        {
            StreamingAssetsManager.ReadAssetBundle(fileUrl, onComplete);
        }
        #endregion


        #endregion

        #region CDN
        /// <summary>
        /// CDN资源版本号
        /// </summary>
        private string m_CDNVersion;

        public string CDNVersion { get { return m_CDNVersion; } }

        /// <summary>
        /// CDN资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_CDNVersionDic;

        /// <summary>
        /// 初始化CDN资源包信息
        /// </summary>
        private void InitCDNAssetBundleInfo()
        {
            string url = string.Format("{0}VersionFile.bytes", GameEntry.Data.SysDataManager.CurrChannelConfig.RealSourceUrl);
			GameEntry.Http.Get(url, OnInitCDNAssetBundleInfo);
        }
        /// <summary>
        /// 初始化CDN资源包信息回调
        /// </summary>
        /// <param name="args"></param>
        private void OnInitCDNAssetBundleInfo(HttpCallBackArgs args)
        {
                m_CDNVersionDic = GetAssetBundleVersionList(args.Data, ref m_CDNVersion);
                GameEntry.Log(LogCategory.Resource, "初始化CDN资源包信息回调=>OnInitCDNAssetBundleInfo()");

                //foreach (var item in m_CDNVersionDic)
                //{
                //    GameEntry.Log(LogCategory.Normal, "item=>" + item.Key);
                //}
                CheckVersionFileExistsInLocal();
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
        private Dictionary<string, AssetBundleInfoEntity> m_LocalAssetsVersionDic;

        /// <summary>
        /// 检查可写区版本文件是否存在
        /// </summary>
        public void CheckVersionFileExistsInLocal()
        {
            GameEntry.Log(LogCategory.Resource, "检查可写区版本文件是否存在=>CheckVersionFileExistsInLocal()");

            if (LocalAssetsManager.GetVersionFileExists())
            {
                //可写区版本文件存在
                //加载可写区资源包信息
                InitLocalAssetBundleInfo();
            }
            else
            {
                //可写区版本文件不存在

                //判断只读区版本文件是否存在
                if (m_IsExistsStreamingAssetsBundleInfo)
                {
                    //只读区版本文件存在
                    //将只读区版本文件初始化到可写区
                    InitVersionFileFormStreamingAssetsToLocal();
                }

                CheckVersionChange();
            }
        }

        /// <summary>
        /// 将只读区版本文件初始化到可写区
        /// </summary>
        private void InitVersionFileFormStreamingAssetsToLocal()
        {
            GameEntry.Log(LogCategory.Resource, "将只读区版本文件初始化到可写区=>InitVersionFileFormStreamingAssetsToLocal()");

            m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();

            var enumerator = m_StreamingAssetsVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity entity = enumerator.Current.Value;
                m_LocalAssetsVersionDic[enumerator.Current.Key] = new AssetBundleInfoEntity()
                {
                    AssetBundleName = entity.AssetBundleName,
                    MD5 = entity.MD5,
                    Size = entity.Size,
                    IsFirstData = entity.IsFirstData,
                    IsEncrypt = entity.IsEncrypt
                };
            }

            //保存版本文件
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDic);

            //保存版本号
            m_LocalAssetsVersion = m_StreamingAssetsVersion;
            LocalAssetsManager.SetResourceVersion(m_LocalAssetsVersion);
        }

        /// <summary>
        /// 初始化可写区资源包信息
        /// </summary>
        private void InitLocalAssetBundleInfo()
        {
            GameEntry.Log(LogCategory.Resource, "初始化可写区资源包信息=>InitLocalAssetBundleInfo()");

            m_LocalAssetsVersionDic = LocalAssetsManager.GetAssetBundleVersionList(ref m_LocalAssetsVersion);

            CheckVersionChange();
        }
        /// <summary>
        /// 保存可写区版本信息
        /// </summary>
        /// <param name="entity"></param>
        public void SaveVersion(AssetBundleInfoEntity entity)
        {
            if (m_LocalAssetsVersionDic == null)
            {
                m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            }
            m_LocalAssetsVersionDic[entity.AssetBundleName] = entity;

            //保存版本文件
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDic);
        }
        /// <summary>
        /// 保存资源版本号(用于检测版本更新完毕后 保存)
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
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        internal AssetBundleInfoEntity GetAssetBundleInfo(string assetBundlePath)
        {
            AssetBundleInfoEntity entity = null;
            m_CDNVersionDic.TryGetValue(assetBundlePath, out entity);
            if (entity == null)
            {
                GameEntry.LogError("获取资源包信息失败=>{0}", assetBundlePath);
            }
            return entity;
        }


        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckVersionChange()
        {
			GameEntry.Log(LogCategory.Resource, "检查更新=>CheckVersionChange(), 版本号=>{0}", m_LocalAssetsVersion);

            if (LocalAssetsManager.GetVersionFileExists())
            {
                if (!string.IsNullOrEmpty(m_LocalAssetsVersion) && m_LocalAssetsVersion.Equals(m_CDNVersion))
                {
                    GameEntry.Log(LogCategory.Resource, "可写区版本号和CDN版本号一致 进入预加载流程");
                    GameEntry.Procedure.ChangeState(ProcedureState.Preload);
                }
                else
                {
                    GameEntry.Log(LogCategory.Resource, "可写区版本号和CDN版本号不一致 开始检查更新");
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
            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionBeginDownload);
            m_DownloadingParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
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

			GameEntry.Log(LogCategory.Resource, "下载初始资源,文件数量==>>" + m_NeedDownloadList.Count);
            GameEntry.Download.BeginDownloadMulit(m_NeedDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
        #endregion

        #region BeginCheckVersionChange 开始检查更新
        /// <summary>
        /// 开始检查更新
        /// </summary>
        private void BeginCheckVersionChange()
        {
            m_DownloadingParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
            m_DownloadingParams.Reset();

            //需要删除的文件
            LinkedList<string> delList = new LinkedList<string>();

            //可写区资源MD5和CDN资源CD5不一致的文件
            LinkedList<string> inconformityList = new LinkedList<string>();
            LinkedList<string> needDownloadList = new LinkedList<string>();

            #region 找出需要删除的可写区文件链表 和 需要下载的可写区文件链表
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
                        //如果MD5不一致 加入不一致链表
                        inconformityList.AddLast(assetBundleName);
                    }
                }
                else
                {
                    //可写区有 CDN上没有 加入删除链表
                    delList.AddLast(assetBundleName);
                }
            }

            //判断这个文件在只读区的MD5和CDN是否一致 一致的要删除 不一致的则重新下载
            LinkedListNode<string> currInconformity = inconformityList.First;
            while (currInconformity != null)
            {
                AssetBundleInfoEntity cdnAssetBundleInfo = null;
                m_CDNVersionDic.TryGetValue(currInconformity.Value, out cdnAssetBundleInfo);

                AssetBundleInfoEntity streamingAssetsAssetBundleInfo = null;
                if (m_StreamingAssetsVersionDic != null)
                {
                    m_StreamingAssetsVersionDic.TryGetValue(currInconformity.Value, out streamingAssetsAssetBundleInfo);
                }

                if (streamingAssetsAssetBundleInfo == null)
                {
                    //如果只读区没有,则重新下载
                    needDownloadList.AddLast(currInconformity.Value);
                }
                else
                {
                    if (cdnAssetBundleInfo.MD5.Equals(streamingAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //一致,则删除
                        delList.AddLast(currInconformity.Value);
                    }
                    else
                    {
                        //不一致,则重新下载
                        needDownloadList.AddLast(currInconformity.Value);
                    }
                }

                currInconformity = currInconformity.Next;
            }
            #endregion

            #region 处理需要删除的可写区文件链表
            LinkedListNode<string> currDel = delList.First;
            while (currDel != null)
            {
                string filePath = string.Format("{0}/{1}", GameEntry.Resource.LocalFilePath, currDel.Value);

                if (File.Exists(filePath)) File.Delete(filePath);

                LinkedListNode<string> next = currDel.Next;
                delList.Remove(currDel);
                currDel = next;
            }
            #endregion

            #region 处理需要下载的可写区文件链表
            enumerator = m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity cdnAssetBundleInfo = enumerator.Current.Value;
                if (cdnAssetBundleInfo.IsFirstData)//是否初始资源
                {
                    if (!m_LocalAssetsVersionDic.ContainsKey(cdnAssetBundleInfo.AssetBundleName))
                    {
                        //如果可写区没有 则去只读区查找
                        AssetBundleInfoEntity streamingAssetsAssetBundleInfo = null;
                        if (m_StreamingAssetsVersionDic != null)
                        {
                            m_StreamingAssetsVersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleName, out streamingAssetsAssetBundleInfo);
                        }
                        if (streamingAssetsAssetBundleInfo == null)
                        {
                            //如果只读区也没有,则去CDN上下载
                            needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                        }
                        else
                        {
                            //如果只读区有 验证MD5
                            if (!cdnAssetBundleInfo.MD5.Equals(streamingAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //MD5不一致,则去CDN上下载
                                needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                            }
                        }
                    }
                }
            }
            #endregion

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionBeginDownload);

            //进行下载
			GameEntry.Log(LogCategory.Resource, "下载更新资源,文件数量==>>" + needDownloadList.Count);
            GameEntry.Download.BeginDownloadMulit(needDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
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

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionDownloadUpdate, m_DownloadingParams);
        }
        /// <summary>
        /// 下载完毕
        /// </summary>
        private void OnDownloadMulitComplete()
        {
            SetResourceVersion();

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionDownloadComplete);
            GameEntry.Pool.EnqueueClassObject(m_DownloadingParams);

            //进入预加载流程
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
        }


        public void Dispose()
        {
            m_StreamingAssetsVersionDic.Clear();
            m_CDNVersionDic.Clear();
            m_LocalAssetsVersionDic.Clear();
        }
	}
}
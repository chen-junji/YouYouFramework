using Cysharp.Threading.Tasks;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YouYouMain;


public class CheckVersionCtrl
{
    public static CheckVersionCtrl Instance { get; private set; } = new CheckVersionCtrl();

    public CheckVersionCtrl()
    {
        m_NeedDownloadList = new LinkedList<string>();
    }

    #region 初始化CDN 可写区 只读区的版本文件信息
    /// <summary>
    /// 初始化CDN 可写区 只读区的版本文件信息
    /// </summary>
    public async UniTask InitVersionFile()
    {
        //去资源站点请求CDN的版本文件信息
        string cdnVersionFileUrl = string.Format("{0}{1}", ChannelModel.Instance.CurrChannelConfig.RealSourceUrl, YFConstDefine.VersionFileName);
        byte[] cdnVersionFileBytes = await WebRequestUtil.LoadCDNBytesAsync(cdnVersionFileUrl);
        if (cdnVersionFileBytes != null)
        {
            //加载CDN版本文件信息
            VersionCDNModel.Instance.VersionDic = LoadVersionFile(cdnVersionFileBytes, ref VersionCDNModel.Instance.Version);
            MainEntry.Log("OnLoadCDNVersionFile");

            //加载可写区版本文件信息
            if (VersionLocalModel.Instance.GetVersionFileExists())
            {
                string json = IOUtil.GetFileText(VersionLocalModel.Instance.VersionFilePath);
                VersionLocalModel.Instance.VersionDic = json.ToObject<Dictionary<string, VersionFileEntity>>();
                VersionLocalModel.Instance.AssetsVersion = PlayerPrefs.GetString(YFConstDefine.AssetVersion);
                MainEntry.Log("OnLoadLocalVersionFile");
            }
            //可写区版本文件不存在
            else
            {
                //加载只读区版本文件信息
                byte[] streamingBuffer = await WebRequestUtil.LoadStreamingBytesAsync(YFConstDefine.VersionFileName);
                if (streamingBuffer != null)
                {
                    VersionStreamingModel.Instance.VersionDic = LoadVersionFile(streamingBuffer, ref VersionStreamingModel.Instance.AssetsVersion);
                    MainEntry.Log("OnStreamingVersionFile");

                    //将只读区版本文件初始化到可写区
                    VersionLocalModel.Instance.VersionDic = new Dictionary<string, VersionFileEntity>();

                    var enumerator = VersionStreamingModel.Instance.VersionDic.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        VersionFileEntity entity = enumerator.Current.Value;
                        VersionLocalModel.Instance.VersionDic[enumerator.Current.Key] = new VersionFileEntity()
                        {
                            AssetBundleName = entity.AssetBundleName,
                            MD5 = entity.MD5,
                            Size = entity.Size,
                            IsFirstData = entity.IsFirstData,
                            IsEncrypt = entity.IsEncrypt
                        };
                    }

                    //保存版本文件
                    VersionLocalModel.Instance.SaveVersion();

                    //保存版本号
                    VersionLocalModel.Instance.SetAssetVersion(VersionStreamingModel.Instance.AssetsVersion);
                }

            }
        }
        else
        {
            MainEntry.Log("初始化CDN资源包信息失败，cdnVersionFileUrl==" + cdnVersionFileUrl);
        }
    }

    public static Dictionary<string, VersionFileEntity> LoadVersionFile(byte[] buffer, ref string version)
    {
        buffer = ZlibHelper.DeCompressBytes(buffer);

        Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();

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
                VersionFileEntity entity = new VersionFileEntity();
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

    #region 检查更新相关逻辑
    /// <summary>
    /// 需要下载的资源包列表
    /// </summary>
    private LinkedList<string> m_NeedDownloadList;

    /// <summary>
    /// 检查版本更新下载时候的参数
    /// </summary>
    private BaseParams m_DownloadingParams;

    public event Action CheckVersionBeginDownload;
    internal event Action<BaseParams> CheckVersionDownloadUpdate;
    public event Action CheckVersionDownloadComplete;

    private Action CheckVersionComplete;

    /// <summary>
    /// 检查更新
    /// </summary>
    public async void CheckVersionChange(Action onComplete)
    {
        CheckVersionComplete = onComplete;

        await InitVersionFile();

        MainEntry.Log("检查更新=>CheckVersionChange(), 版本号=>{0}", VersionLocalModel.Instance.AssetsVersion);

        if (VersionLocalModel.Instance.GetVersionFileExists())
        {
            if (!string.IsNullOrEmpty(VersionLocalModel.Instance.AssetsVersion) && VersionLocalModel.Instance.AssetsVersion.Equals(VersionCDNModel.Instance.Version))
            {
                MainEntry.Log("可写区版本号和CDN版本号一致 不需要检查更新");
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

        var enumerator = VersionCDNModel.Instance.VersionDic.GetEnumerator();
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
        var enumerator = VersionLocalModel.Instance.VersionDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            string assetBundleName = enumerator.Current.Key;
            VersionFileEntity localVersionFile = enumerator.Current.Value;

            if (VersionCDNModel.Instance.VersionDic.TryGetValue(assetBundleName, out VersionFileEntity cdnVersionFile))
            {
                //可写区有 CDN也有
                //判断可写区和CDN的MD5是否一致
                if (!cdnVersionFile.MD5.Equals(localVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (VersionStreamingModel.Instance.VersionDic.TryGetValue(assetBundleName, out VersionFileEntity streamingVersionFile))
                    {
                        //判断只读区和CDN的MD5是否一致
                        if (cdnVersionFile.MD5.Equals(streamingVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase))
                        {
                            //一致,则删除
                            deleteList.AddLast(assetBundleName);
                        }
                        else
                        {
                            //如果MD5不一致 加入下载链表
                            needDownloadList.AddLast(assetBundleName);
                        }
                    }
                    else
                    {
                        //如果只读区没有,则重新下载
                        needDownloadList.AddLast(assetBundleName);
                    }
                }
            }
            else
            {
                //可写区有 CDN上没有
                //加入删除链表
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
        enumerator = VersionCDNModel.Instance.VersionDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            VersionFileEntity cdnVersionFile = enumerator.Current.Value;
            if (cdnVersionFile.IsFirstData)//检查初始资源
            {
                if (!VersionLocalModel.Instance.VersionDic.ContainsKey(cdnVersionFile.AssetBundleName))
                {
                    //如果可写区没有 则去只读区判断一次 
                    if (VersionStreamingModel.Instance.VersionDic.TryGetValue(cdnVersionFile.AssetBundleName, out VersionFileEntity streamingVersionFile))
                    {
                        //只读区存在 验证MD5
                        if (!cdnVersionFile.MD5.Equals(streamingVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase))//MD5不一致
                        {
                            needDownloadList.AddLast(cdnVersionFile.AssetBundleName);
                        }
                    }
                    else
                    {
                        //只读区不存在 加入下载链表
                        needDownloadList.AddLast(cdnVersionFile.AssetBundleName);
                    }
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
        VersionLocalModel.Instance.SetAssetVersion(VersionCDNModel.Instance.Version);

        CheckVersionDownloadComplete?.Invoke();
        //MainEntry.ClassObjectPool.Enqueue(m_DownloadingParams);

        MainEntry.Log("检查更新下载完毕, 进入预加载流程");
        CheckVersionComplete?.Invoke();
    }
    #endregion

}

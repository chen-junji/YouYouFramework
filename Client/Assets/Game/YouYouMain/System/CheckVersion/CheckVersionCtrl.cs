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

    public async UniTask Init()
    {
        //加载只读区版本文件信息
        byte[] streamingBuffer = await LoadUtil.LoadStreamingBytesAsync(MainConstDefine.VersionFileName);
        if (streamingBuffer != null)
        {
            VersionStreamingModel.Instance.VersionDic = LoadUtil.LoadVersionFile(streamingBuffer, ref VersionStreamingModel.Instance.AssetVersion);
            MainEntry.Log("加载只读区版本文件信息");
        }

        //加载可写区版本文件信息
        if (File.Exists(MainConstDefine.LocalVersionFilePath))
        {
            string json = IOUtil.GetFileText(MainConstDefine.LocalVersionFilePath);
            VersionLocalModel.Instance.VersionDic = json.ToObject<Dictionary<string, VersionFileEntity>>();
            VersionLocalModel.Instance.AssetVersion = PlayerPrefs.GetString(MainConstDefine.AssetVersion);
            MainEntry.Log("加载可写区版本文件信息");
        }
    }

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

        //去资源站点请求CDN的版本文件信息
        string cdnVersionFileUrl = Path.Combine(ChannelModel.Instance.CurrChannelConfig.RealSourceUrl, MainConstDefine.VersionFileName);
        MainEntry.Log("请求CDN版本文件，cdnVersionFileUrl==" + cdnVersionFileUrl);
        byte[] cdnVersionFileBytes = await LoadUtil.LoadCDNBytesAsync(cdnVersionFileUrl);
        if (cdnVersionFileBytes != null)
        {
            VersionCDNModel.Instance.VersionDic = LoadUtil.LoadVersionFile(cdnVersionFileBytes, ref VersionCDNModel.Instance.AssetVersion);
            MainEntry.Log("加载CDN版本文件成功，cdnVersionFileUrl==" + cdnVersionFileUrl);
        }
        else
        {
            MainEntry.LogError("请求CDN版本文件失败，请点击重试, cdnVersionFileUrl==" + cdnVersionFileUrl);
            MainDialogForm.ShowForm("There was an error with the network request. Please click to retry.", "Error", "Retry", "", MainDialogForm.DialogFormType.Affirm, () =>
            {
                CheckVersionChange(onComplete);
            });
            return;
        }

        MainEntry.Log($"检查更新=>CheckVersionChange(), 本地版本号=>{VersionLocalModel.Instance.AssetVersion}");

        if (VersionLocalModel.Instance.AssetVersion.Equals(VersionCDNModel.Instance.AssetVersion))
        {
            MainEntry.Log("可写区版本号和CDN版本号一致 不需要检查更新");
            CheckVersionComplete?.Invoke();
        }
        else if (VersionStreamingModel.Instance.AssetVersion.Equals(VersionCDNModel.Instance.AssetVersion))
        {
            MainEntry.Log("只读区版本号和CDN版本号一致 不需要检查更新");
            CheckVersionComplete?.Invoke();
        }
        else
        {
            MainEntry.Log($"CDN版本号=={VersionCDNModel.Instance.AssetVersion}, 和可写区 只读区版本号都不一致 开始检查更新");
            BeginCheckVersionChange();
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
            VersionFileEntity localVersionFile = enumerator.Current.Value;
            if (VersionCDNModel.Instance.VersionDic.TryGetValue(localVersionFile.AssetBundleFullPath, out VersionFileEntity cdnVersionFile))
            {
                if (VersionStreamingModel.Instance.VersionDic.TryGetValue(localVersionFile.AssetBundleFullPath, out VersionFileEntity streamingVersionFile) &&
                cdnVersionFile.MD5.Equals(localVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase) == false &&
                cdnVersionFile.MD5.Equals(streamingVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase))
                {
                    //可写区有 CDN有 只读区有
                    //CDN和可写区的MD5不一致
                    //只读区和CDN的MD5一致
                    //说明可写区的这个文件是旧的, 删除它
                    deleteList.AddLast(localVersionFile.AssetBundleFullPath);
                }
            }
            else
            {
                //可写区有 CDN上没有
                //加入删除链表
                deleteList.AddLast(localVersionFile.AssetBundleFullPath);
            }
        }

        //删除需要删除的
        MainEntry.Log("删除旧资源,文件数量==>" + deleteList.Count + "==>" + deleteList.ToJson());
        LinkedListNode<string> currDel = deleteList.First;
        while (currDel != null)
        {
            string filePath = Path.Combine(MainConstDefine.LocalAssetBundlePath, currDel.Value);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            LinkedListNode<string> next = currDel.Next;
            deleteList.Remove(currDel);
            VersionLocalModel.Instance.VersionDic.Remove(currDel.Value);
            currDel = next;
        }

        //检查需要下载的
        enumerator = VersionCDNModel.Instance.VersionDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            VersionFileEntity cdnVersionFile = enumerator.Current.Value;

            //当前文件不是初始资源 忽略
            if (!cdnVersionFile.IsFirstData) continue;

            if (VersionLocalModel.Instance.VersionDic.TryGetValue(cdnVersionFile.AssetBundleFullPath, out VersionFileEntity localVersionFile))
            {
                //可写区有这个文件
                if (cdnVersionFile.MD5.Equals(localVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    //可写区和CDN的MD5不一致 加入下载链表
                    needDownloadList.AddLast(cdnVersionFile.AssetBundleFullPath);
                }
            }
            else if (VersionStreamingModel.Instance.VersionDic.TryGetValue(cdnVersionFile.AssetBundleFullPath, out VersionFileEntity streamingVersionFile))
            {
                //只读区有这个文件
                if (cdnVersionFile.MD5.Equals(streamingVersionFile.MD5, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    //只读区和CDN的MD5不一致 加入下载链表
                    needDownloadList.AddLast(cdnVersionFile.AssetBundleFullPath);
                }
            }
            else
            {
                //可写区和只读区都不存在 加入下载链表
                needDownloadList.AddLast(cdnVersionFile.AssetBundleFullPath);
            }
        }

        CheckVersionBeginDownload?.Invoke();

        //进行下载
        MainEntry.Log("下载更新资源,文件数量==>" + needDownloadList.Count + "==>" + needDownloadList.ToJson());
        if (needDownloadList.Count > 0)
        {
            MainEntry.Download.BeginDownloadMulit(needDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
        else
        {
            OnDownloadMulitComplete(true);
        }
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
    private void OnDownloadMulitComplete(bool success)
    {
        if (!success)
        {
            MainEntry.LogError("检查更新失败, 下载文件缺失, 请点击重试");
            MainDialogForm.ShowForm("There was an error with the network request. Please click to retry.", "Error", "Retry", "", MainDialogForm.DialogFormType.Affirm, () =>
            {
                CheckVersionChange(CheckVersionComplete);
            });
            return;
        }

        VersionLocalModel.Instance.SetAssetVersion(VersionCDNModel.Instance.AssetVersion);

        CheckVersionDownloadComplete?.Invoke();
        //MainEntry.ClassObjectPool.Enqueue(m_DownloadingParams);

        MainEntry.Log("检查更新下载完毕, 进入预加载流程");
        CheckVersionComplete?.Invoke();
    }
    #endregion

}

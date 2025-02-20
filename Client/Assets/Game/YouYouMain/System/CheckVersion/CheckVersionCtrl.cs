using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YouYouMain;


public class CheckVersionCtrl
{
    public static CheckVersionCtrl Instance { get; private set; } = new CheckVersionCtrl();

    public event Action CheckVersionBeginDownload;
    internal event Action<DownloadStatus> CheckVersionDownloadUpdate;
    public event Action CheckVersionDownloadComplete;

    private Action CheckVersionComplete;

    /// <summary>
    /// 检查更新
    /// </summary>
    public async void CheckVersionChange(Action onComplete)
    {
        CheckVersionComplete = onComplete;

        var updateHandle = Addressables.CheckForCatalogUpdates();
        await updateHandle.Task;
        if (updateHandle.Status == AsyncOperationStatus.Failed)
        {
            //资源清单请求失败
            return;
        }

        if (updateHandle.Result.Count == 0)
        {
            MainEntry.Log("资源清单没变化 不需要检查更新");
            CheckVersionComplete?.Invoke();
            return;
        }
        MainEntry.Log("旧的资源清单==" + updateHandle.Result.ToJson());
        MainEntry.Log("开始更新资源清单");

        // 下载新版本 Catalog 和资源
        var updateOp = Addressables.UpdateCatalogs(updateHandle.Result, false);
        await updateOp.Task;
        MainEntry.Log("资源清单更新完毕==" + updateOp.Result.ToJson());
        updateOp.Release();

        //开始检查更新
        MainEntry.Instance.StartCoroutine(DownloadCoroutine());

    }

    IEnumerator DownloadCoroutine()
    {
        CheckVersionBeginDownload?.Invoke();

        // 开始下载资源及其依赖项
        AsyncOperationHandle _downloadHandle = Addressables.DownloadDependenciesAsync("default");

        // 轮询更新进度
        while (!_downloadHandle.IsDone)
        {
            CheckVersionDownloadUpdate?.Invoke(_downloadHandle.GetDownloadStatus());
            yield return null; // 每帧更新
        }

        // 处理完成状态
        if (_downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(_downloadHandle); // 释放资源句柄

            MainEntry.Log("检查更新下载完毕, 进入预加载流程");
            CheckVersionDownloadComplete?.Invoke();
            CheckVersionComplete?.Invoke();
        }
        else
        {
            MainEntry.LogError("检查更新失败, 请点击重试");
            MainDialogForm.ShowForm("检查更新失败, 请点击重试", "Error", "重试", "", MainDialogForm.DialogFormType.Affirm, () =>
            {
                CheckVersionChange(CheckVersionComplete);
            });
            Debug.Log($"下载失败：{_downloadHandle.OperationException}");
        }
    }
}

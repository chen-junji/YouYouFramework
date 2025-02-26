using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YouYouMain;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif

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

#if UNITY_EDITOR
        // 获取 Addressables 配置
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings.ActivePlayModeDataBuilderIndex == 0)
        {
            MainEntry.Log("编辑器加载模式 不需要检查更新");
            CheckVersionComplete.Invoke();
            return;
        }
#endif

        //自定义新地址
        Addressables.InternalIdTransformFunc = (location) =>
        {
            //Debug.Log($"默认地址=={location.InternalId}");
            if (location.InternalId.StartsWith("http", System.StringComparison.Ordinal))
            {
                string new_location = location.InternalId.Replace("http://test", ChannelModel.Instance.CurrChannelConfig.RealSourceUrl);
                //Debug.Log($"默认地址=={location.InternalId}, 自定义的新地址=={new_location}");
                return new_location;
            }

            return location.InternalId;
        };

        var updateHandle = Addressables.CheckForCatalogUpdates();
        await updateHandle.Task;
        if (updateHandle.Status == AsyncOperationStatus.Failed)
        {
            //资源清单请求失败
            MainEntry.LogError("资源清单请求失败, 请检查ChannelConfigEntity脚本的路径配置");
            return;
        }

        if (updateHandle.Result.Count == 0)
        {
            MainEntry.Log("资源清单没变化 不需要检查更新");
            CheckVersionComplete?.Invoke();
            return;
        }
        MainEntry.Log("旧的资源清单==" + updateHandle.Result.ToJson());

        // 下载新版本 Catalog
        var updateOp = Addressables.UpdateCatalogs(updateHandle.Result, false);
        MainEntry.Log("开始更新资源清单");
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

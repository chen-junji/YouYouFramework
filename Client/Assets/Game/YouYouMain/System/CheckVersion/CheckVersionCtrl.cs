using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YouYouMain;
using System.Collections.Generic;


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
                string new_location = location.InternalId.Replace("http://ChannelConfig", ChannelModel.Instance.CurrChannelConfig.RealSourceUrl);
                //Debug.Log($"默认地址=={location.InternalId}, 自定义的新地址=={new_location}");
                return new_location;
            }

            return location.InternalId;
        };

        var updateHandle = Addressables.CheckForCatalogUpdates(false);
        await updateHandle.Task;
        if (updateHandle.Status == AsyncOperationStatus.Failed)
        {
            //资源清单请求失败
            MainEntry.LogError("资源清单请求失败, 请检查ChannelConfigEntity脚本的路径配置");
            return;
        }
        List<string> catalogsToUpdate = updateHandle.Result;
        updateHandle.Release();

        // 获取所有 keys
        var downloadKeys = new List<object>();
        if (catalogsToUpdate.Count == 0)
        {
            MainEntry.Log("资源清单没变化 不需要更新资源清单");
            // 没有新 Catalog，用当前的 ResourceLocators
            foreach (var locator in Addressables.ResourceLocators)
            {
                downloadKeys.AddRange(locator.Keys);
            }
            //Debug.Log(Addressables.ResourceLocators.ToJson());
        }
        else
        {
            // 下载新版本 Catalog
            var updateOp = Addressables.UpdateCatalogs(true, catalogsToUpdate, false);
            MainEntry.Log("开始更新资源清单");
            await updateOp.Task;
            MainEntry.Log("资源清单更新完毕==" + updateOp.Result.ToJson());

            foreach (var locator in updateOp.Result)
            {
                downloadKeys.AddRange(locator.Keys);
            }
            updateOp.Release();
        }

        if (downloadKeys.Count == 0)
        {
            MainEntry.Log("没有需要下载的资源1");
            CheckVersionComplete?.Invoke();
            return;
        }

        var size = await Addressables.GetDownloadSizeAsync(downloadKeys);
        if (size == 0)
        {
            MainEntry.Log("没有需要下载的资源2");
            CheckVersionComplete?.Invoke();
            return;
        }

        //=========================开始下载更新文件=============================
        CheckVersionBeginDownload?.Invoke();

        // 开始下载资源及其依赖项
        var _downloadHandle = Addressables.DownloadDependenciesAsync(downloadKeys, Addressables.MergeMode.Union);

        // 轮询更新进度
        while (!_downloadHandle.IsDone)
        {
            CheckVersionDownloadUpdate?.Invoke(_downloadHandle.GetDownloadStatus());
            await UniTask.Yield(PlayerLoopTiming.Update); // 每帧更新
        }

        // 处理完成状态
        if (_downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            MainEntry.Log("检查更新下载完毕, 进入预加载流程" + _downloadHandle.Result.ToJson());
            _downloadHandle.Release(); // 释放资源句柄

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

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using YouYouMain;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets.ResourceLocators;


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
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
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

        List<BundleInfo> bundleList = new();
        // 获取所有 keys
        var downloadKeys = new List<object>();
        if (catalogsToUpdate.Count == 0)
        {
            MainEntry.Log("资源清单没变化 不需要更新资源清单");
            foreach (var locator in Addressables.ResourceLocators)
            {
                downloadKeys.AddRange(locator.Keys);
            }
            bundleList = GetAllBundlesAsync(Addressables.ResourceLocators);
        }
        else
        {
            MainEntry.Log("开始更新资源清单");
            var updateOp = Addressables.UpdateCatalogs(true, catalogsToUpdate, false);
            await updateOp.Task;

            foreach (var locator in updateOp.Result)
            {
                downloadKeys.AddRange(locator.Keys);
            }
            bundleList = GetAllBundlesAsync(updateOp.Result);
            updateOp.Release();
        }

        if (bundleList.Count == 0)
        {
            MainEntry.Log("没有需要下载的资源");
            CheckVersionComplete?.Invoke();
            return;
        }
        MainEntry.Log("需要下载的资源列表==" + bundleList.ToJson());

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
            MainEntry.Log("检查更新下载完毕, 进入预加载流程");
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

    /// <summary>
    /// 从 Locator 扫描全部 bundle, 判断是否在缓存内
    /// </summary>
    public static List<BundleInfo> GetAllBundlesAsync(IEnumerable<IResourceLocator> resourceLocators)
    {
        var result = new List<BundleInfo>();
        var checkedBundles = new HashSet<string>();

        foreach (var locator in resourceLocators)
        {
            foreach (var kvp in locator.Keys)
            {
                if (!locator.Locate(kvp, typeof(object), out var locations)) continue;

                foreach (var loc in locations)
                {
                    if (loc.Data is AssetBundleRequestOptions options)
                    {
                        string name = options.BundleName;
                        string hash = options.Hash;
                        if (checkedBundles.Contains(name)) continue;

                        bool cached = Caching.IsVersionCached(name, Hash128.Parse(hash));
                        if (!cached) result.Add(new BundleInfo() { BundleName = name, BundleSize = options.BundleSize, BundleHash = options.Hash });
                        checkedBundles.Add(name);
                    }
                }
            }
        }

        return result;
    }

    public class BundleInfo
    {
        public string BundleName { get; set; }
        public long BundleSize { get; set; }
        public string BundleHash { get; set; }
    }

}

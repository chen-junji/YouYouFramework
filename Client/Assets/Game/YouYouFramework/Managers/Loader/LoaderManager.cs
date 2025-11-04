using YouYouMain;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace YouYouFramework
{
    /// <summary>
    /// 资源加载 管理器
    /// </summary>
    public class LoaderManager
    {
        public LoaderManager()
        {
            //加载时间最短, 但加载时帧率下降最严重
            Application.backgroundLoadingPriority = ThreadPriority.High;
        }

        /// <summary>
        /// 异步加载主资源，自动加载依赖
        /// 注意: 如果这个资源没有打AB包, 则无法加载
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public async UniTask<T> LoadMainAssetAsync<T>(string assetFullPath, GameObject target) where T : Object
        {
            if (target == null)
            {
                GameEntry.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            var referenceEntity = Addressables.LoadAssetAsync<T>(assetFullPath);
            await referenceEntity.Task;
            AssetReleaseHandle.Add(referenceEntity, target);
            return referenceEntity.Result;
        }

        /// <summary>
        /// 同步加载主资源, 自动加载依赖
        /// 注意: 如果这个资源没有打AB包, 则无法加载
        /// 注意: 微信小游戏不支持同步加载
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public T LoadMainAsset<T>(string assetFullPath, GameObject target) where T : Object
        {
            if (target == null)
            {
                GameEntry.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            var op = Addressables.LoadAssetAsync<T>(assetFullPath);
            AssetReleaseHandle.Add(op, target);
            return op.WaitForCompletion();
        }

    }
}
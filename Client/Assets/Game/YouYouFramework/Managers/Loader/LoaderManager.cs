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
        /// 要注意这个资源有没有打AB包, 它不能被依赖打包, 否则无法加载
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public async UniTask<T> LoadMainAssetAsync<T>(string assetFullPath, GameObject target, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null) where T : Object
        {
            if (target == null)
            {
                GameEntry.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            AsyncOperationHandle referenceEntity = await LoadMainAssetAsync(assetFullPath, onUpdate, onDownloadUpdate);
            AssetReleaseHandle.Add(referenceEntity, target);
            return referenceEntity.Result as T;
        }
        /// <summary>
        /// 异步加载主资源，自动加载依赖， 注意：这个方法需要自己调用AssetReferenceEntity.ReferenceAdd去管理引用计数
        /// 要注意这个资源有没有打AB包, 它不能被依赖打包, 否则无法加载
        /// </summary>
        public async UniTask<AsyncOperationHandle> LoadMainAssetAsync(string assetFullPath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            var op = Addressables.LoadAssetAsync<Object>(assetFullPath);
            await op.Task;
            return op;
        }

        /// <summary>
        /// 同步加载主资源, 自动加载依赖
        /// 要注意这个资源有没有打AB包, 它不能被依赖打包, 否则无法加载
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
        /// <summary>
        /// 同步加载主资源, 自动加载依赖， 注意：这个方法需要自己调用AssetReferenceEntity.ReferenceAdd去管理引用计数
        /// 要注意这个资源有没有打AB包, 它不能被依赖打包, 否则无法加载
        /// </summary>
        public AsyncOperationHandle LoadMainAsset(string assetFullPath)
        {
            var op = Addressables.LoadAssetAsync<Object>(assetFullPath);
            op.WaitForCompletion();
            return op;
        }

    }
}
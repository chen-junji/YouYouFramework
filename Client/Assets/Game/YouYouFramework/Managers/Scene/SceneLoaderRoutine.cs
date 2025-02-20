using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using YouYouMain;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;


namespace YouYouFramework
{
    /// <summary>
    /// 场景加载和卸载器
    /// </summary>
    public class SceneLoaderRoutine
    {
        private AsyncOperation m_CurrAsync = null;

        public string SceneFullPath;

        private AsyncOperationHandle<SceneInstance> asyncOperation;

        /// <summary>
        /// 进度更新
        /// </summary>
        private Action<string, float> OnProgressUpdate;

        /// <summary>
        /// 加载场景
        /// </summary>
        public async void LoadScene(string sceneFullPath, Action<string, float> onProgressUpdate)
        {
            SceneFullPath = sceneFullPath;

            OnProgressUpdate = onProgressUpdate;

            asyncOperation = Addressables.LoadSceneAsync(sceneFullPath, LoadSceneMode.Additive);
            await asyncOperation.Task;
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public async void UnLoadScene()
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(asyncOperation);
            await handle.Task;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            if (m_CurrAsync == null) return;
            if (!m_CurrAsync.isDone)
            {
                OnProgressUpdate?.Invoke(SceneFullPath, m_CurrAsync.progress);
            }
        }
    }
}
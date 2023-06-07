using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YouYou;
using System;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 场景加载和卸载器
    /// </summary>
    public class SceneLoaderRoutine
    {
        private AsyncOperation m_CurrAsync = null;

        private string SceneName;

        /// <summary>
        /// 进度更新
        /// </summary>
        private Action<string, float> OnProgressUpdate;

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneDetailId"></param>
        /// <param name="sceneName"></param>
        /// <param name="onProgressUpdate"></param>
        /// <param name="onComplete"></param>
        public async void LoadScene(string sceneName, Action<string, float> onProgressUpdate)
        {
            SceneName = sceneName;

            OnProgressUpdate = onProgressUpdate;

#if EDITORLOAD || RESOURCES
            m_CurrAsync = SceneManager.LoadSceneAsync(sceneName , LoadSceneMode.Additive);
#else
            //加载场景的资源包
            await GameEntry.Resource.LoadMainAssetAsync(sceneName);
            m_CurrAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
#endif
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="onComplete"></param>
        public void UnLoadScene(string sceneName)
        {
            m_CurrAsync = SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            if (m_CurrAsync == null) return;
            if (!m_CurrAsync.isDone)
            {
                OnProgressUpdate?.Invoke(SceneName, m_CurrAsync.progress);
            }
        }
    }
}
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
        /// 加载场景完毕
        /// </summary>
        private Action<SceneLoaderRoutine> OnLoadSceneComplete;

        /// <summary>
        /// 卸载场景完毕
        /// </summary>
        private Action<SceneLoaderRoutine> OnUnLoadSceneComplete;

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneDetailId"></param>
        /// <param name="sceneName"></param>
        /// <param name="onProgressUpdate"></param>
        /// <param name="onComplete"></param>
        public async void LoadScene(string sceneName, Action<string, float> onProgressUpdate, Action<SceneLoaderRoutine> onLoadSceneComplete)
        {
            Reset();

            SceneName = sceneName;

            OnProgressUpdate = onProgressUpdate;
            OnLoadSceneComplete = onLoadSceneComplete;

#if EDITORLOAD || RESOURCES
            m_CurrAsync = SceneManager.LoadSceneAsync("Assets/Game/Download/" + sceneName + ".unity", LoadSceneMode.Additive);
            m_CurrAsync.allowSceneActivation = false;
            if (m_CurrAsync == null) OnLoadSceneComplete?.Invoke(this);
#else
            //加载场景的资源包
            Object obj = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<Object>(sceneName + ".unity");
            m_CurrAsync = SceneManager.LoadSceneAsync("Assets/Game/Download/" + sceneName + ".unity", LoadSceneMode.Additive);
            m_CurrAsync.allowSceneActivation = false;
            if (m_CurrAsync == null) OnLoadSceneComplete?.Invoke(this);
#endif
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="onComplete"></param>
        public void UnLoadScene(string sceneName, Action<SceneLoaderRoutine> onUnLoadSceneComplete)
        {
            Reset();
            OnUnLoadSceneComplete = onUnLoadSceneComplete;
            m_CurrAsync = SceneManager.UnloadSceneAsync(sceneName);
            if (m_CurrAsync == null) OnUnLoadSceneComplete?.Invoke(this);
        }

        private void Reset()
        {
            m_CurrAsync = null;
            OnProgressUpdate = null;
            OnLoadSceneComplete = null;
            OnUnLoadSceneComplete = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            if (m_CurrAsync == null) return;

            if (!m_CurrAsync.isDone)
            {
                if (m_CurrAsync.progress >= 0.9f)
                {
                    OnProgressUpdate?.Invoke(SceneName, m_CurrAsync.progress);
                    m_CurrAsync.allowSceneActivation = true;
                    m_CurrAsync = null;
                    OnLoadSceneComplete?.Invoke(this);
                }
                else
                {
                    OnProgressUpdate?.Invoke(SceneName, m_CurrAsync.progress);
                }
            }
            else
            {
                m_CurrAsync = null;
                OnUnLoadSceneComplete?.Invoke(this);
            }
        }
    }
}
//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YouYou;

namespace YouYou
{
	/// <summary>
	/// 场景加载和卸载器
	/// </summary>
	public class SceneLoaderRoutine
	{
		private AsyncOperation m_CurrAsync = null;

		/// <summary>
		/// 进度更新
		/// </summary>
		private BaseAction<int, float> OnProgressUpdate;

		/// <summary>
		/// 加载场景完毕
		/// </summary>
		private BaseAction<SceneLoaderRoutine> OnLoadSceneComplete;

		/// <summary>
		/// 卸载场景完毕
		/// </summary>
		private BaseAction<SceneLoaderRoutine> OnUnLoadSceneComplete;

		/// <summary>
		/// 场景明细编号
		/// </summary>
		private int m_SceneDetailId;

		/// <summary>
		/// 加载场景
		/// </summary>
		/// <param name="sceneDetailId"></param>
		/// <param name="sceneName"></param>
		/// <param name="onProgressUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadScene(int sceneDetailId, string sceneName, BaseAction<int, float> onProgressUpdate, BaseAction<SceneLoaderRoutine> onLoadSceneComplete)
		{
			Reset();

			m_SceneDetailId = sceneDetailId;
			OnProgressUpdate = onProgressUpdate;
			OnLoadSceneComplete = onLoadSceneComplete;

#if EDITORLOAD || RESOURCES
			m_CurrAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			m_CurrAsync.allowSceneActivation = false;
			if (m_CurrAsync == null) OnLoadSceneComplete?.Invoke(this);
#else
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(GameEntry.Resource.GetSceneAssetBundlePath(sceneName), onComplete: (AssetBundle bundle2) =>
			{
				//加载场景的资源包
				m_CurrAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				m_CurrAsync.allowSceneActivation = false;
				if (m_CurrAsync == null) OnLoadSceneComplete?.Invoke(this);
			});
#endif
		}

		/// <summary>
		/// 卸载场景
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="onComplete"></param>
		public void UnLoadScene(string sceneName, BaseAction<SceneLoaderRoutine> onUnLoadSceneComplete)
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
					OnProgressUpdate?.Invoke(m_SceneDetailId, m_CurrAsync.progress);
					m_CurrAsync.allowSceneActivation = true;
					m_CurrAsync = null;
					OnLoadSceneComplete?.Invoke(this);
				}
				else
				{
					OnProgressUpdate?.Invoke(m_SceneDetailId, m_CurrAsync.progress);
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
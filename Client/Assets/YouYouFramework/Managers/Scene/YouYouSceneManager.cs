using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YouYou
{
	/// <summary>
	/// 场景管理器
	/// </summary>
	public class YouYouSceneManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// 场景加载器链表
		/// </summary>
		private LinkedList<SceneLoaderRoutine> m_SceneLoaderList;

		/// <summary>
		/// 当前加载的场景编号
		/// </summary>
		private int m_CurrLoadSceneId;

		/// <summary>
		/// 当前场景数据实体
		/// </summary>
		public Sys_SceneEntity m_CurrSceneEntity { get; private set; }

		/// <summary>
		/// 当前场景明细
		/// </summary>
		private List<Sys_SceneDetailEntity> m_CurrSceneDetailList;

		/// <summary>
		/// 需要加载或者卸载的明细数量
		/// </summary>
		private int m_NeedLoadOrUnloadSceneDetailCount = 0;

		/// <summary>
		/// 当前已经加载或者卸载的明细数量
		/// </summary>
		private int m_CurrLoadOrUnloadSceneDetailCount = 0;

		/// <summary>
		/// 场景是否加载中
		/// </summary>
		private bool m_CurrSceneIsLoading;

		/// <summary>
		/// 当前进度
		/// </summary>
		private float m_CurrProgress = 0;

		/// <summary>
		/// 目标的进度
		/// </summary>
		private Dictionary<int, float> m_TargetProgressDic;

		/// <summary>
		/// 加载场景的参数
		/// </summary>
		private BaseParams m_CurrLoadingParam;

		/// <summary>
		/// 加载完毕委托
		/// </summary>
		private BaseAction m_OnComplete = null;

		internal YouYouSceneManager()
		{
			m_SceneLoaderList = new LinkedList<SceneLoaderRoutine>();
			m_TargetProgressDic = new Dictionary<int, float>();
		}

		internal override void Init()
		{

		}

		/// <summary>
		/// 加载场景
		/// </summary>
		/// <param name="sceneId">场景编号</param>
		/// <param name="showLoadingForm">是否显示Loading</param>
		/// <param name="onComplete">加载完毕</param>
		public void LoadScene(int sceneId, bool showLoadingForm = false, BaseAction onComplete = null)
		{
			if (m_CurrSceneIsLoading)
			{
				GameEntry.LogError("场景{0}正在加载中", m_CurrLoadSceneId);
				return;
			}

			if (m_CurrLoadSceneId == sceneId)
			{
				GameEntry.LogError("正在重复加载场景{0}", sceneId);
				m_OnComplete?.Invoke();
				return;
			}

			//停止BGM
			GameEntry.Audio.StopBGM();

			m_CurrLoadingParam = GameEntry.Pool.DequeueClassObject<BaseParams>();
			m_OnComplete = onComplete;

			if (showLoadingForm)
			{
				//加载Loading
				GameEntry.UI.OpenUIForm(UIFormId.UI_Loading, onOpen: (UIFormBase form) =>
				{
					DoLoadScene(sceneId);
				});
			}
			else
			{
				DoLoadScene(sceneId);
			}
		}

		/// <summary>
		/// 执行加载场景
		/// </summary>
		/// <param name="sceneId"></param>
		private void DoLoadScene(int sceneId)
		{
			m_CurrProgress = 0;
			m_TargetProgressDic.Clear();

			m_CurrSceneIsLoading = true;
			m_CurrLoadSceneId = sceneId;
			//先现在当前的场景
			UnLoadCurrScene();
		}

		/// <summary>
		/// 卸载当前场景
		/// </summary>
		private void UnLoadCurrScene()
		{
			if (m_CurrSceneEntity != null)
			{
				m_NeedLoadOrUnloadSceneDetailCount = m_CurrSceneDetailList.Count;
				for (int i = 0; i < m_NeedLoadOrUnloadSceneDetailCount; i++)
				{
					SceneLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<SceneLoaderRoutine>();
					m_SceneLoaderList.AddLast(routine);
					routine.UnLoadScene(m_CurrSceneDetailList[i].ScenePath, OnUnLoadSceneComplete);
				}
			}
			else
			{
				LoadNewScene();
			}
		}

		/// <summary>
		/// 加载新场景
		/// </summary>
		private void LoadNewScene()
		{
			m_CurrSceneEntity = GameEntry.DataTable.Sys_SceneDBModel.GetDic(m_CurrLoadSceneId);
			m_CurrSceneDetailList = GameEntry.DataTable.Sys_SceneDetailDBModel.GetListBySceneId(m_CurrSceneEntity.Id, 2);
			m_NeedLoadOrUnloadSceneDetailCount = m_CurrSceneDetailList.Count;

			for (int i = 0; i < m_NeedLoadOrUnloadSceneDetailCount; i++)
			{
				SceneLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<SceneLoaderRoutine>();
				m_SceneLoaderList.AddLast(routine);

				Sys_SceneDetailEntity entity = m_CurrSceneDetailList[i];
				routine.LoadScene(entity.Id, entity.ScenePath, OnLoadSceneProgressUpdate, OnLoadSceneComplete);
			}
		}

		private void OnLoadSceneComplete(SceneLoaderRoutine routine)
		{
			m_SceneLoaderList.Remove(routine);
			GameEntry.Pool.EnqueueClassObject(routine);
		}

		private void OnLoadSceneProgressUpdate(int sceneDetailId, float progress)
		{
			//记录每个场景明细当前的进度
			m_TargetProgressDic[sceneDetailId] = progress;
		}

		private void OnUnLoadSceneComplete(SceneLoaderRoutine routine)
		{
			m_SceneLoaderList.Remove(routine);
			GameEntry.Pool.EnqueueClassObject(routine);

			m_CurrLoadOrUnloadSceneDetailCount++;
			if (m_CurrLoadOrUnloadSceneDetailCount == m_NeedLoadOrUnloadSceneDetailCount)
			{
#if UNLOADRES_CHANGESCENE
                if (LuaManager.luaEnv != null)
                {
                    LuaManager.luaEnv.FullGc();
                }
                Resources.UnloadUnusedAssets();
#endif
				m_NeedLoadOrUnloadSceneDetailCount = 0;
				m_CurrLoadOrUnloadSceneDetailCount = 0;
				LoadNewScene();
			}
		}

		/// <summary>
		/// 更新
		/// </summary>
		internal void OnUpdate()
		{
			if (m_CurrSceneIsLoading)
			{
				var curr = m_SceneLoaderList.First;
				while (curr != null)
				{
					curr.Value.OnUpdate();
					curr = curr.Next;
				}

				float currTarget = GetCurrTotalProgress();
				float finalTarget = 0.9f * m_NeedLoadOrUnloadSceneDetailCount;
				if (currTarget >= finalTarget)
				{
					currTarget = m_NeedLoadOrUnloadSceneDetailCount;
				}

				if (m_CurrProgress < m_NeedLoadOrUnloadSceneDetailCount && m_CurrProgress <= currTarget)
				{
					m_CurrProgress = m_CurrProgress + Time.deltaTime * m_NeedLoadOrUnloadSceneDetailCount * 1;
					m_CurrLoadingParam.IntParam1 = (int)LoadingType.ChangeScene;
					m_CurrLoadingParam.FloatParam1 = Math.Min(m_CurrProgress / m_NeedLoadOrUnloadSceneDetailCount, 1);

					GameEntry.Event.CommonEvent.Dispatch(SysEventId.LoadingProgressChange, m_CurrLoadingParam);
				}
				else if (m_CurrProgress >= m_NeedLoadOrUnloadSceneDetailCount)
				{
					SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_CurrSceneDetailList[0].ScenePath));
					GameEntry.Log(LogCategory.Normal, "场景加载完毕");

					//播放BGM
					GameEntry.Audio.PlayBGM(m_CurrSceneEntity.BGMId);

					m_NeedLoadOrUnloadSceneDetailCount = 0;
					m_CurrLoadOrUnloadSceneDetailCount = 0;
					m_CurrSceneIsLoading = false;
					GameEntry.UI.CloseUIForm(UIFormId.UI_Loading);

					m_CurrLoadingParam.Reset();
					GameEntry.Pool.EnqueueClassObject(m_CurrLoadingParam);

					if (m_OnComplete != null)
					{
						m_OnComplete();
					}
				}
			}
		}

		/// <summary>
		/// 获取当前加载的总进度
		/// </summary>
		/// <returns></returns>
		private float GetCurrTotalProgress()
		{
			float progress = 0;
			var lst = m_TargetProgressDic.GetEnumerator();
			while (lst.MoveNext())
			{
				progress += lst.Current.Value;
			}
			return progress;
		}

		public void Dispose()
		{

		}
	}
}
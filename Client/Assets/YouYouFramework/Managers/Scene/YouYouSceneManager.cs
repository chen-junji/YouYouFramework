using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YouYou
{
	/// <summary>
	/// ����������
	/// </summary>
	public class YouYouSceneManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// ��������������
		/// </summary>
		private LinkedList<SceneLoaderRoutine> m_SceneLoaderList;

		/// <summary>
		/// ��ǰ���صĳ������
		/// </summary>
		private int m_CurrLoadSceneId;

		/// <summary>
		/// ��ǰ��������ʵ��
		/// </summary>
		private Sys_SceneEntity m_CurrSceneEntity;

		/// <summary>
		/// ��ǰ������ϸ
		/// </summary>
		private List<Sys_SceneDetailEntity> m_CurrSceneDetailList;

		/// <summary>
		/// ��Ҫ���ػ���ж�ص���ϸ����
		/// </summary>
		private int m_NeedLoadOrUnloadSceneDetailCount = 0;

		/// <summary>
		/// ��ǰ�Ѿ����ػ���ж�ص���ϸ����
		/// </summary>
		private int m_CurrLoadOrUnloadSceneDetailCount = 0;

		/// <summary>
		/// �����Ƿ������
		/// </summary>
		private bool m_CurrSceneIsLoading;

		/// <summary>
		/// ��ǰ����
		/// </summary>
		private float m_CurrProgress = 0;

		/// <summary>
		/// Ŀ��Ľ���
		/// </summary>
		private Dictionary<int, float> m_TargetProgressDic;

		/// <summary>
		/// ���س����Ĳ���
		/// </summary>
		private BaseParams m_CurrLoadingParam;

		/// <summary>
		/// �������ί��
		/// </summary>
		private BaseAction m_OnComplete = null;

		public YouYouSceneManager()
		{
			m_SceneLoaderList = new LinkedList<SceneLoaderRoutine>();
			m_TargetProgressDic = new Dictionary<int, float>();
		}

		public override void Init()
		{

		}

		/// <summary>
		/// ���س���
		/// </summary>
		/// <param name="sceneId">�������</param>
		/// <param name="showLoadingForm">�Ƿ���ʾLoading</param>
		/// <param name="onComplete">�������</param>
		public void LoadScene(int sceneId, bool showLoadingForm = false, BaseAction onComplete = null)
		{
			if (m_CurrSceneIsLoading)
			{
				GameEntry.LogError("����{0}���ڼ�����", m_CurrLoadSceneId);
				return;
			}

			if (m_CurrLoadSceneId == sceneId)
			{
				GameEntry.LogError("�����ظ����س���{0}", sceneId);
				m_OnComplete?.Invoke();
				return;
			}

			//ֹͣBGM
			GameEntry.Audio.StopBGM();

			m_CurrLoadingParam = GameEntry.Pool.DequeueClassObject<BaseParams>();
			m_OnComplete = onComplete;

			if (showLoadingForm)
			{
				//����Loading
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
		/// ִ�м��س���
		/// </summary>
		/// <param name="sceneId"></param>
		private void DoLoadScene(int sceneId)
		{
			m_CurrProgress = 0;
			m_TargetProgressDic.Clear();

			m_CurrSceneIsLoading = true;
			m_CurrLoadSceneId = sceneId;
			//�����ڵ�ǰ�ĳ���
			UnLoadCurrScene();
		}

		/// <summary>
		/// ж�ص�ǰ����
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
		/// �����³���
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
			//��¼ÿ��������ϸ��ǰ�Ľ���
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
		/// ����
		/// </summary>
		public void OnUpdate()
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
					GameEntry.Log(LogCategory.Normal, "�����������");

					//����BGM
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
		/// ��ȡ��ǰ���ص��ܽ���
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
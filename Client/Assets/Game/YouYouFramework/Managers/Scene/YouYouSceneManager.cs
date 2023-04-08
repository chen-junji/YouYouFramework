using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YouYou
{
    /// <summary>
    /// 场景管理器
    /// </summary>
    public class YouYouSceneManager : IDisposable
    {
        /// <summary>
        /// 场景加载器链表
        /// </summary>
        private LinkedList<SceneLoaderRoutine> m_SceneLoaderList;

        /// <summary>
        /// 当前加载的场景编号
        /// </summary>
        private string m_CurrLoadSceneName;

        /// <summary>
        /// 当前场景明细
        /// </summary>
        private List<Sys_SceneEntity> m_CurrSceneDetailList;

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
        private Action m_OnComplete = null;

        internal YouYouSceneManager()
        {
            m_CurrSceneDetailList = new List<Sys_SceneEntity>();
            m_SceneLoaderList = new LinkedList<SceneLoaderRoutine>();
            m_TargetProgressDic = new Dictionary<int, float>();
        }

        internal void Init()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMode) =>
            {
                if (m_CurrSceneDetailList == null || m_CurrSceneDetailList.Count == 0) return;

                //设置列表里的第一个场景为主场景(激活场景)
                if (m_CurrLoadOrUnloadSceneDetailCount == 0)
                {
                    SceneManager.SetActiveScene(scene);
                    //初始化对象池
                    GameEntry.Pool.GameObjectPool.Init();
                }

                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == m_CurrSceneDetailList.Count)
                {
                    GC.Collect();
                    GameEntry.Log(LogCategory.Normal, "场景加载完毕{0}", m_CurrSceneDetailList[0].SceneName);

                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    m_CurrSceneIsLoading = false;

                    m_CurrLoadingParam.Reset();
                    GameEntry.Pool.EnqueueClassObject(m_CurrLoadingParam);

                    m_OnComplete?.Invoke();
                    //GameEntry.UI.CloseUIForm(UIFormId.UI_Loading);
                }
            };
            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                if (SceneManager.sceneCount == 2)
                {
                    Resources.UnloadUnusedAssets();
                }
            };
        }

        public void UnLoadAllScene()
        {
            if (m_CurrSceneDetailList.Count == 0) return;
            for (int i = 0; i < m_CurrSceneDetailList.Count; i++)
            {
                SceneLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.UnLoadScene(m_CurrSceneDetailList[i].ScenePath, (SceneLoaderRoutine retRoutine) =>
                {
                    m_SceneLoaderList.Remove(retRoutine);
                    GameEntry.Pool.EnqueueClassObject(retRoutine);
                });
            }
            m_CurrSceneDetailList.Clear();
            m_CurrLoadSceneName = null;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadSceneAction(string sceneName, Action onComplete = null)
        {
            if (m_CurrSceneIsLoading)
            {
                GameEntry.LogError("场景{0}正在加载中", m_CurrLoadSceneName);
                return;
            }

            m_OnComplete = onComplete;
            if (m_CurrLoadSceneName == sceneName)
            {
                GameEntry.LogError("正在重复加载场景{0}", sceneName);
                m_OnComplete?.Invoke();
                return;
            }

            m_CurrLoadingParam = GameEntry.Pool.DequeueClassObject<BaseParams>();

            DoLoadScene(sceneName);
        }
        public async ETTask LoadScene(string sceneName)
        {
            ETTask task = ETTask.Create();
            LoadSceneAction(sceneName, task.SetResult);
            await task;
        }

        /// <summary>
        /// 执行加载场景
        /// </summary>
        private void DoLoadScene(string sceneName)
        {
            m_CurrProgress = 0;
            m_TargetProgressDic.Clear();

            m_CurrLoadSceneName = sceneName;
            UnLoadCurrScene();
        }
        /// <summary>
        /// 卸载当前场景并加载新场景
        /// </summary>
        private void UnLoadCurrScene()
        {
            if (m_CurrSceneDetailList.Count > 0)
            {
                for (int i = 0; i < m_CurrSceneDetailList.Count; i++)
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
            m_CurrSceneIsLoading = true;
            m_CurrSceneDetailList = GameEntry.DataTable.Sys_SceneDBModel.GetListBySceneName(m_CurrLoadSceneName);

            for (int i = 0; i < m_CurrSceneDetailList.Count; i++)
            {
                Sys_SceneEntity entity = m_CurrSceneDetailList[i];

                SceneLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.LoadScene(entity.Id, entity.ScenePath, (int sceneDetailId, float progress) =>
                {
                    //记录每个场景明细当前的进度
                    m_TargetProgressDic[sceneDetailId] = progress;
                }, (SceneLoaderRoutine retRoutine) =>
                {
                    m_SceneLoaderList.Remove(retRoutine);
                    GameEntry.Pool.EnqueueClassObject(retRoutine);
                });
            }
        }
        private void OnUnLoadSceneComplete(SceneLoaderRoutine routine)
        {
            m_SceneLoaderList.Remove(routine);
            GameEntry.Pool.EnqueueClassObject(routine);

            m_CurrLoadOrUnloadSceneDetailCount++;
            if (m_CurrLoadOrUnloadSceneDetailCount == m_CurrSceneDetailList.Count)
            {
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
                float finalTarget = 0.9f * m_CurrSceneDetailList.Count;
                if (currTarget >= finalTarget)
                {
                    currTarget = m_CurrSceneDetailList.Count;
                }

                if (m_CurrProgress <= m_CurrSceneDetailList.Count && m_CurrProgress <= currTarget)
                {
                    m_CurrProgress += Time.deltaTime * m_CurrSceneDetailList.Count * 1;
                    m_CurrLoadingParam.IntParam1 = (int)LoadingType.ChangeScene;
                    m_CurrLoadingParam.FloatParam1 = Math.Min(m_CurrProgress / m_CurrSceneDetailList.Count, 1);

                    GameEntry.Event.Common.Dispatch(CommonEventId.LoadingProgressChange, m_CurrLoadingParam);
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
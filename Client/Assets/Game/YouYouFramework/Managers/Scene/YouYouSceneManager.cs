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
        private int m_CurrLoadSceneId;

        /// <summary>
        /// 当前场景数据实体
        /// </summary>
        public Sys_SceneEntity CurrSceneEntity { get; private set; }

        /// <summary>
        /// 当前场景明细
        /// </summary>
        private List<Sys_SceneDetailEntity> m_CurrSceneDetailList;

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
            m_SceneLoaderList = new LinkedList<SceneLoaderRoutine>();
            m_TargetProgressDic = new Dictionary<int, float>();
        }

        internal void Init()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMode) =>
            {
                if (m_CurrSceneDetailList == null || m_CurrSceneDetailList.Count == 0) return;

                //设置列表里的第一个场景为主场景(激活场景)
                if (m_CurrLoadOrUnloadSceneDetailCount == 0) SceneManager.SetActiveScene(scene);

                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == m_CurrSceneDetailList.Count)
                {
                    GameEntry.Log(LogCategory.Normal, "场景加载完毕{0}", CurrSceneEntity.SceneName);

                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    m_CurrSceneIsLoading = false;

                    m_CurrLoadingParam.Reset();
                    GameEntry.Pool.EnqueueClassObject(m_CurrLoadingParam);

                    m_OnComplete?.Invoke();
                    //GameEntry.UI.CloseUIForm(UIFormId.UI_Loading);
                }
            };
        }

        public void UnLoadAllScene()
        {
            if (CurrSceneEntity != null)
            {
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
                CurrSceneEntity = null;
                m_CurrLoadSceneId = 0;
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneId">场景编号</param>
        /// <param name="showLoadingForm">是否显示Loading</param>
        /// <param name="onComplete">加载完毕</param>
        public async void LoadScene(int sceneId, bool showLoadingForm = false, Action onComplete = null)
        {
            if (m_CurrSceneIsLoading)
            {
                GameEntry.LogError("场景{0}正在加载中", m_CurrLoadSceneId);
                return;
            }

            m_OnComplete = onComplete;
            if (m_CurrLoadSceneId == sceneId)
            {
                GameEntry.LogError("正在重复加载场景{0}", sceneId);
                m_OnComplete?.Invoke();
                return;
            }

            m_CurrLoadingParam = GameEntry.Pool.DequeueClassObject<BaseParams>();

            if (showLoadingForm)
            {
                //加载Loading
                await GameEntry.UI.OpenUIForm<UILoadingForm>(UIFormId.UILoading);
                DoLoadScene(sceneId);
            }
            else
            {
                DoLoadScene(sceneId);
            }
        }
        public async ETTask LoadSceneAsync(int sceneId, bool showLoadingForm = false)
        {
            ETTask task = ETTask.Create();
            LoadScene(sceneId, showLoadingForm, task.SetResult);
            await task;
        }

        /// <summary>
        /// 执行加载场景
        /// </summary>
        /// <param name="sceneId"></param>
        private void DoLoadScene(int sceneId)
        {
            m_CurrProgress = 0;
            m_TargetProgressDic.Clear();

            m_CurrLoadSceneId = sceneId;
            UnLoadCurrScene();
        }
        /// <summary>
        /// 卸载当前场景并加载新场景
        /// </summary>
        private void UnLoadCurrScene()
        {
            if (CurrSceneEntity != null)
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
            CurrSceneEntity = GameEntry.DataTable.Sys_SceneDBModel.GetDic(m_CurrLoadSceneId);
            m_CurrSceneDetailList = GameEntry.DataTable.Sys_SceneDetailDBModel.GetListBySceneId(CurrSceneEntity.SceneName, 2);

            for (int i = 0; i < m_CurrSceneDetailList.Count; i++)
            {
                Sys_SceneDetailEntity entity = m_CurrSceneDetailList[i];

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
#if UNLOADRES_CHANGESCENE
                if (LuaManager.luaEnv != null)
                {
                    LuaManager.luaEnv.FullGc();
                }
                Resources.UnloadUnusedAssets();
#endif
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

                    GameEntry.Event.CommonEvent.Dispatch(CommonEventId.LoadingProgressChange, m_CurrLoadingParam);
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
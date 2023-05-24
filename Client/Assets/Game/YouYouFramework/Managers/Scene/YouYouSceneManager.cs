using Main;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YouYou
{
    /// <summary>
    /// 场景管理器
    /// </summary>
    public class YouYouSceneManager
    {
        /// <summary>
        /// 场景加载器链表
        /// </summary>
        private LinkedList<SceneLoaderRoutine> m_SceneLoaderList;

        /// <summary>
        /// 当前加载的场景编号
        /// </summary>
        private SceneGroupName m_CurrLoadSceneName;

        /// <summary>
        /// 当前场景组
        /// </summary>
        public SceneEntity CurrSceneEntity { get; private set; }

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
        private Dictionary<string, float> m_TargetProgressDic;

        /// <summary>
        /// 加载完毕委托
        /// </summary>
        private Action m_OnComplete = null;

        internal YouYouSceneManager()
        {
            m_SceneLoaderList = new LinkedList<SceneLoaderRoutine>();
            m_TargetProgressDic = new Dictionary<string, float>();
        }

        internal void Init()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMode) =>
            {
                if (CurrSceneEntity.AssetPathList.Count == 0) return;

                //设置列表里的第一个场景为主场景(激活场景)
                if (m_CurrLoadOrUnloadSceneDetailCount == 0)
                {
                    SceneManager.SetActiveScene(scene);
                    //初始化对象池
                    GameEntry.Pool.GameObjectPool.Init();
                }

                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == CurrSceneEntity.AssetPathList.Count)
                {
                    GameEntry.Log(LogCategory.Scene, "场景加载完毕=={0}", CurrSceneEntity.ToJson());

                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    m_CurrSceneIsLoading = false;

                    m_OnComplete?.Invoke();
                    //GameEntry.UI.CloseUIForm(UIFormId.UI_Loading);
                }
            };
            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                if (CurrSceneEntity.AssetPathList.Count == 0) return;

                if (SceneManager.sceneCount == 2)
                {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                }
                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == CurrSceneEntity.AssetPathList.Count)
                {
                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    LoadNewScene();
                }
            };
        }

        public async ETTask LoadScene(SceneGroupName sceneName)
        {
            ETTask task = ETTask.Create();
            LoadSceneAction(sceneName, task.SetResult);
            await task;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadSceneAction(SceneGroupName sceneName, Action onComplete = null)
        {
            if (m_CurrSceneIsLoading)
            {
                GameEntry.LogError(LogCategory.Framework, "场景{0}正在加载中", m_CurrLoadSceneName);
                return;
            }

            m_OnComplete = onComplete;
            if (m_CurrLoadSceneName == sceneName)
            {
                GameEntry.LogError(LogCategory.Framework, "正在重复加载场景{0}", sceneName);
                m_OnComplete?.Invoke();
                return;
            }

            m_CurrProgress = 0;
            m_TargetProgressDic.Clear();
            m_CurrLoadSceneName = sceneName;

            //卸载当前场景并加载新场景
            if (CurrSceneEntity.SceneGroupName != SceneGroupName.None && CurrSceneEntity.AssetPathList.Count > 0)
            {
                for (int i = 0; i < CurrSceneEntity.AssetPathList.Count; i++)
                {
                    SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                    m_SceneLoaderList.AddLast(routine);
                    routine.UnLoadScene(CurrSceneEntity.AssetPathList[i], (SceneLoaderRoutine routine) =>
                    {
                        m_SceneLoaderList.Remove(routine);
                        MainEntry.ClassObjectPool.Enqueue(routine);
                    });
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
            CurrSceneEntity = SceneConst.GetDic(m_CurrLoadSceneName);

            for (int i = 0; i < CurrSceneEntity.AssetPathList.Count; i++)
            {
                SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.LoadScene(CurrSceneEntity.AssetPathList[i], (string sceneDetailId, float progress) =>
                {
                    //记录每个场景明细当前的进度
                    m_TargetProgressDic[sceneDetailId] = progress;
                }, (SceneLoaderRoutine retRoutine) =>
                {
                    m_SceneLoaderList.Remove(retRoutine);
                    MainEntry.ClassObjectPool.Enqueue(retRoutine);
                });
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
                float finalTarget = 0.9f * CurrSceneEntity.AssetPathList.Count;
                if (currTarget >= finalTarget)
                {
                    currTarget = CurrSceneEntity.AssetPathList.Count;
                }

                if (m_CurrProgress <= CurrSceneEntity.AssetPathList.Count && m_CurrProgress <= currTarget)
                {
                    m_CurrProgress += Time.deltaTime * CurrSceneEntity.AssetPathList.Count * 1;
                    MainEntry.Data.Dispatch(SysDataMgr.EventName.LoadingSceneUpdate, Math.Min(m_CurrProgress / CurrSceneEntity.AssetPathList.Count, 1));
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

        public void UnLoadAllScene()
        {
            if (CurrSceneEntity.AssetPathList.Count == 0) return;
            for (int i = 0; i < CurrSceneEntity.AssetPathList.Count; i++)
            {
                SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.UnLoadScene(CurrSceneEntity.AssetPathList[i], (SceneLoaderRoutine retRoutine) =>
                {
                    m_SceneLoaderList.Remove(retRoutine);
                    MainEntry.ClassObjectPool.Enqueue(retRoutine);
                });
            }
            m_CurrLoadSceneName = SceneGroupName.None;
        }

    }
}
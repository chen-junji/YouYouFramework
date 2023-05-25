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
        /// 当前加载的场景组
        /// </summary>
        private SceneGroupName m_CurrSceneGroupName;

        /// <summary>
        /// 当前场景组
        /// </summary>
        public List<Sys_SceneEntity> CurrSceneEntityGroup { get; private set; }

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
            CurrSceneEntityGroup = new List<Sys_SceneEntity>();
        }

        internal void Init()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMode) =>
            {
                if (CurrSceneEntityGroup.Count == 0) return;

                //设置列表里的第一个场景为主场景(激活场景)
                if (m_CurrLoadOrUnloadSceneDetailCount == 0)
                {
                    SceneManager.SetActiveScene(scene);
                    //初始化对象池
                    GameEntry.Pool.GameObjectPool.Init();
                }

                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == CurrSceneEntityGroup.Count)
                {
                    GameEntry.Log(LogCategory.Scene, "场景加载完毕=={0}", CurrSceneEntityGroup.ToJson());

                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    m_CurrSceneIsLoading = false;

                    m_OnComplete?.Invoke();
                    //GameEntry.UI.CloseUIForm(UIFormId.UI_Loading);
                }
            };
            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                if (CurrSceneEntityGroup.Count == 0) return;

                m_CurrLoadOrUnloadSceneDetailCount++;
                if (m_CurrLoadOrUnloadSceneDetailCount == CurrSceneEntityGroup.Count)
                {
                    m_CurrLoadOrUnloadSceneDetailCount = 0;
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
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
                GameEntry.LogError(LogCategory.Framework, "场景{0}正在加载中", m_CurrSceneGroupName);
                return;
            }

            m_OnComplete = onComplete;
            if (m_CurrSceneGroupName == sceneName)
            {
                GameEntry.LogError(LogCategory.Framework, "正在重复加载场景{0}", sceneName);
                m_OnComplete?.Invoke();
                return;
            }

            m_CurrProgress = 0;
            m_TargetProgressDic.Clear();
            m_CurrSceneGroupName = sceneName;

            //卸载当前场景并加载新场景
            if (CurrSceneEntityGroup.Count > 0)
            {
                for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
                {
                    SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                    m_SceneLoaderList.AddLast(routine);
                    routine.UnLoadScene(CurrSceneEntityGroup[i].ScenePath, (SceneLoaderRoutine routine) =>
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
            CurrSceneEntityGroup = GameEntry.DataTable.Sys_SceneDBModel.GetListByGroupName(m_CurrSceneGroupName.ToString());

            for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
            {
                SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.LoadScene(CurrSceneEntityGroup[i].ScenePath, (string sceneDetailId, float progress) =>
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
                float finalTarget = 0.9f * CurrSceneEntityGroup.Count;
                if (currTarget >= finalTarget)
                {
                    currTarget = CurrSceneEntityGroup.Count;
                }

                if (m_CurrProgress <= CurrSceneEntityGroup.Count && m_CurrProgress <= currTarget)
                {
                    m_CurrProgress += Time.deltaTime * CurrSceneEntityGroup.Count * 1;
                    MainEntry.Data.Dispatch(SysDataMgr.EventName.LoadingSceneUpdate, Math.Min(m_CurrProgress / CurrSceneEntityGroup.Count, 1));
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

        public void UnLoadCurrScene()
        {
            if (CurrSceneEntityGroup.Count == 0) return;
            for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
            {
                SceneLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<SceneLoaderRoutine>();
                m_SceneLoaderList.AddLast(routine);
                routine.UnLoadScene(CurrSceneEntityGroup[i].ScenePath, (SceneLoaderRoutine retRoutine) =>
                {
                    m_SceneLoaderList.Remove(retRoutine);
                    MainEntry.ClassObjectPool.Enqueue(retRoutine);
                });
            }
            m_CurrSceneGroupName = SceneGroupName.None;
            CurrSceneEntityGroup.Clear();
        }

    }
}
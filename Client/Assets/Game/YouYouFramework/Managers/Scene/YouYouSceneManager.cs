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
        /// 本次场景加载的最大数量
        /// </summary>
        private int SceneLoadMaxCount;

        /// <summary>
        /// 当前场景组
        /// </summary>
        public List<Sys_SceneEntity> CurrSceneEntityGroup { get; private set; }

        /// <summary>
        /// 当前已经加载或者卸载的明细数量
        /// </summary>
        private int m_CurrUnloadSceneDetailCount = 0;

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
                if (scene.path == CurrSceneEntityGroup[0].ScenePath)
                {
                    SceneManager.SetActiveScene(scene);
                    //初始化对象池
                    GameEntry.Pool.GameObjectPool.InitScenePool();
                }

                m_TargetProgressDic[scene.path] = 1;
            };
            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                if (CurrSceneEntityGroup.Count == 0) return;

                m_CurrUnloadSceneDetailCount++;
                if (m_CurrUnloadSceneDetailCount == CurrSceneEntityGroup.Count)
                {
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    LoadNewScene();
                }
            };
        }

        public async ETTask LoadSceneAsync(SceneGroupName sceneName, int sceneLoadCount = -1)
        {
            ETTask task = ETTask.Create();
            LoadSceneAction(sceneName, sceneLoadCount, task.SetResult);
            await task;
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadSceneAction(SceneGroupName sceneName, int sceneLoadCount = -1, Action onComplete = null)
        {
            if (m_CurrSceneIsLoading)
            {
                GameEntry.LogError(LogCategory.Framework, "场景{0}正在加载中", m_CurrSceneGroupName);
                return;
            }
            m_CurrSceneIsLoading = true;

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
            SceneLoadMaxCount = sceneLoadCount;

            //卸载当前场景并加载新场景
            if (CurrSceneEntityGroup.Count > 0)
            {
                m_CurrUnloadSceneDetailCount = 0;
                for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
                {
                    SceneLoaderRoutine routine = new SceneLoaderRoutine();
                    routine.UnLoadScene(CurrSceneEntityGroup[i].ScenePath);
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
            m_SceneLoaderList.Clear();
            CurrSceneEntityGroup = GameEntry.DataTable.Sys_SceneDBModel.GetListByGroupName(m_CurrSceneGroupName.ToString(), SceneLoadMaxCount);

            for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
            {
                SceneLoaderRoutine routine = new SceneLoaderRoutine();
                m_SceneLoaderList.AddLast(routine);
                routine.LoadScene(CurrSceneEntityGroup[i].ScenePath, (string sceneDetailId, float progress) =>
                {
                    //记录每个场景明细当前的进度
                    m_TargetProgressDic[sceneDetailId] = progress;
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

                //模拟加载进度条
                float targetProgress = GetCurrTotalProgress();
                if (m_CurrProgress < targetProgress)
                {
                    //根据实际情况调节速度, 加载已完成和未完成, 模拟进度增值速度分开计算!
                    if (targetProgress < 1)
                    {
                        m_CurrProgress += Time.deltaTime * 0.5f;
                    }
                    else
                    {
                        m_CurrProgress += Time.deltaTime * 0.8f;
                    }
                    m_CurrProgress = Mathf.Min(m_CurrProgress, targetProgress);//这里是为了防止进度超过100%， 比如完成了显示102%
                    MainEntry.Data.LoadingSceneUpdate(m_CurrProgress);
                }

                if (m_CurrProgress == 1)
                {
                    GameEntry.Log(LogCategory.Scene, "场景加载完毕=={0}", CurrSceneEntityGroup.ToJson());
                    m_CurrSceneIsLoading = false;
                    m_OnComplete?.Invoke();
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
            progress /= m_TargetProgressDic.Count;
            return progress;
        }

        public void UnLoadCurrScene()
        {
            if (CurrSceneEntityGroup.Count == 0) return;
            for (int i = 0; i < CurrSceneEntityGroup.Count; i++)
            {
                SceneLoaderRoutine routine = new SceneLoaderRoutine();
                routine.UnLoadScene(CurrSceneEntityGroup[i].ScenePath);
            }
            m_CurrSceneGroupName = SceneGroupName.None;
            CurrSceneEntityGroup.Clear();
        }

    }
}
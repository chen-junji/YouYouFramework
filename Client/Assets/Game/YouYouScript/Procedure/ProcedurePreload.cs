using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 预加载流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        /// <summary>
        /// 目标进度(实际进度)
        /// </summary>
        private float m_TargetProgress;
        /// <summary>
        /// 当前进度(模拟进度)
        /// </summary>
        private float m_CurrProgress;

        /// <summary>
        /// 预加载参数
        /// </summary>
        private BaseParams m_PreloadParams;

        internal override void OnEnter()
        {
            base.OnEnter();
            m_PreloadParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
            m_PreloadParams.Reset();
            GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadBegin);

            m_CurrProgress = 0;

            BeginTask();
        }
        internal override void OnUpdate()
        {
            base.OnUpdate();

            //模拟加载进度条
            if (m_CurrProgress < m_TargetProgress)
            {
                //根据实际情况调节速度, 加载已完成和未完成, 模拟进度增值速度分开计算!
                if (m_TargetProgress < 100)
                {
                    m_CurrProgress = Mathf.Min(m_CurrProgress + Time.deltaTime * 30, m_TargetProgress - 1);//-1是为了防止模拟加载比实际加载快
                }
                else
                {
                    m_CurrProgress = Mathf.Min(m_CurrProgress + Time.deltaTime * 60, m_TargetProgress);
                }
                m_PreloadParams.FloatParam1 = m_CurrProgress;
                GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadUpdate, m_PreloadParams);
            }

            if (m_CurrProgress == 100)
            {
                GameEntry.Event.CommonEvent.Dispatch(CommonEventId.PreloadComplete);
                GameEntry.Pool.EnqueueClassObject(m_PreloadParams);

                //进入到业务流程
                GameEntry.Procedure.ChangeState(ProcedureState.Game);
            }
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        private async void BeginTask()
        {
            TaskGroup taskGroup = GameEntry.Task.CreateTaskGroup();
#if ASSETBUNDLE
            //初始化资源信息
            await GameEntry.Resource.InitAssetInfo();

            //加载自定义Shader
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.CusShadersAssetBundlePath, onComplete: (AssetBundle bundle) =>
                {
                    bundle.LoadAllAssets();
                    Shader.WarmupAllShaders();
                    taskRoutine.Leave();
                });
            });
#endif
            //加载Excel
            await GameEntry.DataTable.LoadDataAllTableAsync();

            //加载初始UI
            List<Sys_UIFormEntity> lst = GameEntry.DataTable.Sys_UIFormDBModel.GetList().FindAll(x => x.LoadType == 2);
            for (int i = 0; i < lst.Count; i++)
            {
                Sys_UIFormEntity entity = lst[i];
                taskGroup.AddTask((taskRoutine) => GameEntry.UI.PreloadUI(entity, taskRoutine.Leave));
            }

            //加载初始声音
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Audio.FMOD.LoadBanks(taskRoutine.Leave);
            });

            //初始化ILRuntime
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.ILRuntime.Init();
                GameEntry.ILRuntime.OnLoadDataTableComplete = () => taskRoutine.Leave();
            });

            taskGroup.OnCompleteOne = () =>
            {
                m_TargetProgress = taskGroup.CurrCount / (float)taskGroup.TotalCount * 100;
            };
            taskGroup.Run();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYou
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    public class TaskRoutine
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int TaskRoutineId;

        /// <summary>
        /// 具体的任务
        /// </summary>
        public Action<TaskRoutine> CurrTask;

        /// <summary>
        /// 任务完成
        /// </summary>
        public event Action OnComplete;

        /// <summary>
        /// 停止任务
        /// </summary>
        public Action StopTask;

        /// <summary>
        /// 任务数据
        /// </summary>
        public object TaskData;

        /// <summary>
        /// 进入任务
        /// </summary>
        public void Enter()
        {
            if (CurrTask != null) CurrTask(this);
            else Leave();
        }
        public void OnUpdate()
        {
            if (CurrTask != null && CurrTask.Target == null) Leave();
        }

        /// <summary>
        /// 离开任务
        /// </summary>
        public void Leave()
        {
            if (OnComplete != null)
            {
                OnComplete();
                OnComplete = null;
                CurrTask = null;
            }
        }
    }
}
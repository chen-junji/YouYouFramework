using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYouFramework
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
        /// 任务开始
        /// </summary>
        public void TaskBegin()
        {
            if (CurrTask != null) CurrTask(this);
            else TaskComplete();
        }
        /// <summary>
        /// 任务完成
        /// </summary>
        public void TaskComplete()
        {
            if (OnComplete != null)
            {
                OnComplete();
                OnComplete = null;
                CurrTask = null;
            }
        }

        public void OnUpdate()
        {
            if (CurrTask != null && CurrTask.Target == null) TaskComplete();
        }
    }
}
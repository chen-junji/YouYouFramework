using System;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 任务组
    /// </summary>
    public class TaskGroup : IDisposable
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        private LinkedList<TaskRoutine> m_TaskRoutineList;

        /// <summary>
        /// 任务组完成
        /// </summary>
        public Action OnComplete;
        /// <summary>
        /// 单个任务完成
        /// </summary>
        public Action OnCompleteOne;

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool InTask { get; private set; }

        public int TotalCount { get; private set; }
        public int CurrCount { get; private set; }

        public TaskGroup()
        {
            m_TaskRoutineList = new LinkedList<TaskRoutine>();
        }
        public void OnUpdate()
        {
            LinkedListNode<TaskRoutine> taskRotine = m_TaskRoutineList.First;
            while (taskRotine != null)
            {
                taskRotine.Value.OnUpdate();
                taskRotine = taskRotine.Next;
            }
        }
        public void Dispose()
        {
            InTask = false;
            OnComplete?.Invoke();
            CurrCount = 0;
            TotalCount = 0;
            m_TaskRoutineList.Clear();
            GameEntry.Task.RemoveTaskGroup(this);
        }

        public virtual void AddTask(Action<TaskRoutine> task)
        {
            if (task == null) return;
            TaskRoutine taskRoutine = new();
            taskRoutine.CurrTask = task;
            taskRoutine.TaskIndex = TotalCount;

            m_TaskRoutineList.AddLast(taskRoutine);
            TotalCount++;
        }

        public void LeaveCurrTask()
        {
            LinkedListNode<TaskRoutine> curr = m_TaskRoutineList.First;
            if (curr != null && InTask)
            {
                curr.Value.TaskComplete();
            }
        }

        /// <summary>
        /// 清空所有任务
        /// </summary>
        public void ClearAllTask()
        {
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                var next = routine.Next;
                routine.Value.StopTask?.Invoke();
                m_TaskRoutineList.Remove(routine);
                routine = next;
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Run(Action onStart = null)
        {
            if (m_TaskRoutineList.Count == 0) return;

            if (InTask) return;
            InTask = true;

            GameEntry.Task.RegisterTaskGroup(this);
            onStart?.Invoke();

            CheckTask();
        }

        /// <summary>
        /// 按照AddTask顺序执行任务
        /// </summary>
        private void CheckTask()
        {
            LinkedListNode<TaskRoutine> curr = m_TaskRoutineList.First;
            if (curr != null)
            {
                curr.Value.OnComplete += () =>
                {
                    CurrCount++;
                    OnCompleteOne?.Invoke();
                    m_TaskRoutineList.Remove(curr);
                    CheckTask();
                };
                curr.Value.TaskBegin();
            }
            else
            {
                Dispose();
            }
        }

        public void LogTask()
        {
            Debug.LogError("======================");
            Debug.LogError(string.Format("InTask={0}", InTask));
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                Debug.LogError(routine.Value);
                Debug.LogError(routine.Value.CurrTask);
                Debug.LogError(string.Format("{0}=========={1}", routine.Value.CurrTask.Target, routine.Value.CurrTask.Method));
                routine = routine.Next;
            }
        }
    }
}
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
        /// 是否并发执行
        /// </summary>
        private bool m_IsConcurrency = false;

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

        public void Dispose()
        {
            InTask = false;
            OnComplete?.Invoke();
            CurrCount = 0;
            TotalCount = 0;
            m_TaskRoutineList.Clear();
            GameEntry.Task.RemoveTaskGroup(this);
        }

        public virtual void AddTask(Action<TaskRoutine> task, bool isAddGroup = true)
        {
            if (task == null) return;
            TaskRoutine taskRoutine = new TaskRoutine();
            taskRoutine.CurrTask = task;
            if (isAddGroup)
            {
                m_TaskRoutineList.AddLast(taskRoutine);
                TotalCount++;
            }
            else
            {
                taskRoutine.TaskBegin();
            }
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
        /// <param name="isConcurrency">是否并行</param>
        /// <param name="onStart"></param>
        public void Run(bool isConcurrency = false, Action onStart = null)
        {
            if (m_TaskRoutineList.Count == 0) return;

            if (InTask) return;
            InTask = true;

            GameEntry.Task.RegisterTaskGroup(this);
            onStart?.Invoke();

            //是否并行
            m_IsConcurrency = isConcurrency;
            if (m_IsConcurrency)
            {
                ConcurrencyTask();
            }
            else
            {
                CheckTask();
            }
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

        /// <summary>
        /// 并发执行任务
        /// </summary>
        private void ConcurrencyTask()
        {
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                LinkedListNode<TaskRoutine> next = routine.Next;
                routine.Value.OnComplete += () =>
                {
                    CurrCount++;
                    OnCompleteOne?.Invoke();
                    if (CurrCount == TotalCount) Dispose();
                };
                routine.Value.TaskBegin();
                routine = next;
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
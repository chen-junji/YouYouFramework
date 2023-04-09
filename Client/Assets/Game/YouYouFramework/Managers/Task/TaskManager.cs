using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskManager : IDisposable
    {
        /// <summary>
        /// 任务组列表
        /// </summary>
        private LinkedList<TaskGroup> m_TaskGroupList;

        private TaskGroup CommonGroup;

        public TaskManager()
        {
            m_TaskGroupList = new LinkedList<TaskGroup>();
            CommonGroup = new TaskGroup();
        }

        internal void Init()
        {
            CommonGroup.OnComplete = () =>
            {
                GameEntry.UI.CloseUIForm(UIFormId.UICircle);
            };
        }
        public void OnUpdate()
        {
#if DEBUG_MODEL
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.E))
            {
                CommonGroup.LogTask();
                LinkedListNode<TaskGroup> taskGroup = m_TaskGroupList.First;
                while (taskGroup != null)
                {
                    YouYou.GameEntry.LogError(LogCategory.Framework, "======================");
                    taskGroup.Value.LogTask();
                    taskGroup = taskGroup.Next;
                }
            }
#endif
            UpdateItem();
        }

        private void UpdateItem()
        {
            CommonGroup.OnUpdate();
            LinkedListNode<TaskGroup> taskGroup = m_TaskGroupList.First;
            while (taskGroup != null)
            {
                taskGroup.Value.OnUpdate();
                taskGroup = taskGroup.Next;
            }
        }

        /// <summary>
        /// 添加异步任务 (等待异步时会有 转圈等待UI遮罩)
        /// </summary>
        public void AddTaskCommon(Action<TaskRoutine> task, bool isTask = true)
        {
            CommonGroup.AddTask(task, isTask);
            CommonGroup.Run(false, () => GameEntry.UI.OpenUIFormAction<UIBase>(UIFormId.UICircle));
        }

        /// <summary>
        /// 创建一个任务组
        /// </summary>
        /// <returns></returns>
        public TaskGroup CreateTaskGroup()
        {
            TaskGroup taskGroup = new TaskGroup();
            return taskGroup;
        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        /// <param name="action"></param>
        internal void RegisterTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.AddLast(taskGroup);
        }
        /// <summary>
        /// 移除任务组
        /// </summary>
        /// <param name="taskGroup"></param>
        public void RemoveTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.Remove(taskGroup);
        }

        public void Dispose()
        {

        }
    }
}
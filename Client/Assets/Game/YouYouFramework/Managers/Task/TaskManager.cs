using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// 任务组列表
        /// </summary>
        private LinkedList<TaskGroup> m_TaskGroupList;

        public TaskManager()
        {
            m_TaskGroupList = new LinkedList<TaskGroup>();
        }
        public void OnUpdate()
        {
            LinkedListNode<TaskGroup> taskGroup = m_TaskGroupList.First;
            while (taskGroup != null)
            {
                taskGroup.Value.OnUpdate();
                taskGroup = taskGroup.Next;

                if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.E))
                {
                    taskGroup.Value.LogTask();
                }
            }
        }

        /// <summary>
        /// 创建一个任务组
        /// </summary>
        public TaskGroup CreateTaskGroup()
        {
            TaskGroup taskGroup = new TaskGroup();
            return taskGroup;
        }

        /// <summary>
        /// 注册任务组
        /// </summary>
        internal void RegisterTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.AddLast(taskGroup);
        }
        /// <summary>
        /// 移除任务组
        /// </summary>
        public void RemoveTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.Remove(taskGroup);
        }
    }
}
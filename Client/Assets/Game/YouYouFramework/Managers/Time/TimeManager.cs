using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class TimeManager 
    {
        /// <summary>
        /// 定时器排序字典
        /// </summary>
        private readonly SortedDictionary<float, List<TimeAction>> m_SortedDictionary;
        private readonly Queue<float> timeOutTime = new Queue<float>();
        // 记录最近的TimeAction的临时目标时间点
        private float minTime;

        internal TimeManager()
        {
            m_SortedDictionary = new SortedDictionary<float, List<TimeAction>>();
        }

        internal void Init()
        {

        }

        /// <summary>
        /// 注册定时器
        /// </summary>
        internal void Register(float tillTime, TimeAction action)
        {
            if (!m_SortedDictionary.ContainsKey(tillTime)) m_SortedDictionary.Add(tillTime, new List<TimeAction>());

            m_SortedDictionary[tillTime].Add(action);
            if (tillTime < minTime) minTime = tillTime;
        }
        /// <summary>
        /// 移除定时器
        /// </summary>
        internal void Remove(float tillTime, TimeAction action)
        {
            List<TimeAction> lst = null;
            if (m_SortedDictionary.TryGetValue(tillTime, out lst))
            {
                lst.Remove(action);
            }
        }

        internal void OnUpdate()
        {
            if (m_SortedDictionary.Count == 0) return;
            if (Time.time < minTime) return;

            foreach (var item in m_SortedDictionary)
            {
                float k = item.Key;
                if (k > Time.time)
                {
                    minTime = item.Key;
                    break;
                }
                timeOutTime.Enqueue(k);
            }
            foreach (var item in timeOutTime)
            {
                List<TimeAction> lst = m_SortedDictionary[item];
                for (int i = 0; i < lst.Count; i++)
                {
                    lst[i].TillTimeEnd();
                }
                m_SortedDictionary.Remove(item);
            }
            timeOutTime.Clear();
        }

        /// <summary>
        /// 创建定时器
        /// </summary>
        /// <returns></returns>
        public TimeAction Create(string timeName = null, float delayTime = 0, float interval = 1, int loop = 0, Action onStar = null, Action<int> onUpdate = null, Action onComplete = null)
        {
            return new TimeAction().Init(timeName, delayTime, interval, loop, onStar, onUpdate, onComplete);
        }
        public async ETTask Delay(float delayTime)
        {
            ETTask task = ETTask.Create();
            Create(delayTime: delayTime, onStar: task.SetResult);
            await task;
        }

        /// <summary>
        /// 延迟一帧
        /// </summary>
        /// <param name="onComplete"></param>
        public void Yield(Action onComplete)
        {
            GameEntry.Instance.StartCoroutine(YieldCoroutine());
            IEnumerator YieldCoroutine()
            {
                yield return null;
                if (onComplete != null) onComplete();
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 定时器
    /// </summary>
    public class TimeAction
    {
        /// <summary>
        /// 定时器的名字
        /// </summary>
        public string TimeName { get; private set; }

        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool IsStart { get; private set; }

        /// <summary>
        /// 临时目标时间点
        /// </summary>
        private float tillTime;

        /// <summary>
        /// 当前循环次数
        /// </summary>
        private int m_CurrLoop;

        /// <summary>
        /// 间隔（秒）
        /// </summary>
        private float m_Interval;

        /// <summary>
        /// 循环次数(-1表示 无限循环 0也会循环一次)
        /// </summary>
        private int m_Loop;

        /// <summary>
        /// 最后暂停时间
        /// </summary>
        private float m_LastPauseTime;

        /// <summary>
        /// 开始运行
        /// </summary>
        public Action OnStartAction { get; private set; }

        /// <summary>
        /// 运行中 回调参数表示剩余次数
        /// </summary>
        public Action<int> OnUpdateAction { get; private set; }

        /// <summary>
        /// 运行完毕
        /// </summary>
        public Action OnCompleteAction { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="timeName">定时器名字</param>
        /// <param name="delayTime">延迟时间</param>
        /// <param name="interval">间隔</param>
        /// <param name="loop">循环次数</param>
        internal TimeAction Init(string timeName = null, float delayTime = 0, float interval = 1, int loop = 0, Action onStar = null, Action<int> onUpdate = null, Action onComplete = null)
        {
            if (tillTime > 0)
            {
                Debug.LogError("定时器正在使用中");
                return null;
            }
            TimeName = timeName;
            //m_DelayTime = delayTime;
            m_Interval = interval;
            m_Loop = loop;
            OnStartAction = onStar;
            OnUpdateAction = onUpdate;
            OnCompleteAction = onComplete;

            tillTime = Time.time + delayTime;
            m_CurrLoop = 0;
            GameEntry.Time.Register(tillTime, this);

            return this;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            GameEntry.Time.Remove(tillTime, this);
            IsStart = false;
            OnStartAction = null;
            OnUpdateAction = null;
            OnCompleteAction = null;
            tillTime = 0;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            m_LastPauseTime = Time.time;
            GameEntry.Time.Remove(tillTime, this);
        }
        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            //计算暂停了多久
            tillTime += Time.time - m_LastPauseTime;
            GameEntry.Time.Register(tillTime, this);
        }

        /// <summary>
        /// 时间到达
        /// </summary>
        public void TillTimeEnd()
        {
            if (!IsStart)
            {
                if (OnStartAction != null && (OnStartAction.Target == null || OnStartAction.Target.ToString() == "null")) return;
                OnStartAction?.Invoke();
            }
            else
            {
                tillTime = Time.time + m_Interval;
            }
            IsStart = true;

            //以下代码 间隔m_Interval 时间 执行一次
            if (OnUpdateAction != null && (OnUpdateAction.Target == null || OnUpdateAction.Target.ToString() == "null")) return;
            OnUpdateAction?.Invoke(m_Loop - m_CurrLoop);
            if (m_Loop != -1)
            {
                if (m_CurrLoop >= m_Loop)
                {
                    if (OnCompleteAction != null && (OnCompleteAction.Target == null || OnCompleteAction.Target.ToString() == "null")) return;
                    OnCompleteAction?.Invoke();
                    return;
                }
                m_CurrLoop++;
            }
            GameEntry.Time.Register(tillTime, this);
        }
    }
}
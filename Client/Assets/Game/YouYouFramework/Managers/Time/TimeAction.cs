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
        public string TimeName
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool IsRuning
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool m_IsPause = false;

        /// <summary>
        /// 当前运行的时间
        /// </summary>
        private float m_CurrRunTime;

        /// <summary>
        /// 当前循环次数
        /// </summary>
        private int m_CurrLoop;

        /// <summary>
        /// 延迟时间
        /// </summary>
        private float m_DelayTime;

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
        /// 暂停了多久
        /// </summary>
        private float m_PauseTime;

        /// <summary>
        /// 开始运行
        /// </summary>
        public Action OnStarAction
        {
            get;
            private set;
        }

        /// <summary>
        /// 运行中 回调参数表示剩余次数
        /// </summary>
        public Action<int> OnUpdateAction
        {
            get;
            private set;
        }

        /// <summary>
        /// 运行完毕
        /// </summary>
        public Action OnCompleteAction
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="timeName">定时器名字</param>
        /// <param name="delayTime">延迟时间</param>
        /// <param name="interval">间隔</param>
        /// <param name="loop">循环次数</param>
        /// <param name="onStar"></param>
        /// <param name="onUpdate"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public TimeAction Init(string timeName = null, float delayTime = 0, float interval = 1, int loop = 0,
            Action onStar = null, Action<int> onUpdate = null, Action onComplete = null)
        {
            TimeName = timeName;
            m_DelayTime = delayTime;
            m_Interval = interval;
            m_Loop = loop;
            OnStarAction = onStar;
            OnUpdateAction = onUpdate;
            OnCompleteAction = onComplete;

            return this;
        }

        /// <summary>
        /// 运行
        /// </summary>
        public TimeAction Run()
        {
            //1.需要先把自己加入时间管理器的链表中
            if (!IsRuning) GameEntry.Time.RegisterTimeAction(this);

            //2.设置当前运行的时间
            m_CurrRunTime = Time.time;
            m_CurrLoop = 0;
            m_IsPause = false;

            return this;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            IsRuning = false;
            OnStarAction = null;
            OnUpdateAction = null;
            OnCompleteAction = null;

            //把自己从定时器链表移除
            GameEntry.Time.RemoveTimeAction(this);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            m_LastPauseTime = Time.time;
            m_IsPause = true;
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            m_IsPause = false;

            //计算暂停了多久
            m_PauseTime = Time.time - m_LastPauseTime;
        }


        internal void OnUpdate()
        {
            if (m_IsPause) return;

            //1.等待延迟时间
            if (Time.time > m_CurrRunTime + m_PauseTime + m_DelayTime)
            {
                if (!IsRuning)
                {
                    //开始运行
                    m_CurrRunTime = Time.time;
                    m_PauseTime = 0;
                    OnStarAction?.Invoke();
                }
                IsRuning = true;
            }
            if (!IsRuning) return;

            if (Time.time > m_CurrRunTime + m_PauseTime)
            {
                m_CurrRunTime = Time.time + m_Interval;
                m_PauseTime = 0;
                //以下代码 间隔m_Interval 时间 执行一次
                OnUpdateAction?.Invoke(m_Loop - m_CurrLoop);

                if (m_Loop != -1)
                {
                    if (m_CurrLoop >= m_Loop)
                    {
                        OnCompleteAction?.Invoke();
                        Stop();
                    }
                    m_CurrLoop++;
                }
            }
        }
    }
}
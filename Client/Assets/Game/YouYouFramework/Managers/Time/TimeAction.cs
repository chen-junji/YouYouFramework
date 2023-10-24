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
        /// 目标循环次数(-1表示 无限循环 0也会循环一次)
        /// </summary>
        private int m_Loop;

        /// <summary>
        /// 最后暂停时间
        /// </summary>
        private float m_LastPauseTime;

        /// <summary>
        /// 是否无视时间缩放 TODO
        /// </summary>
        private bool Unscaled;

        /// <summary>
        /// 运行中 回调参数表示剩余次数
        /// </summary>
        public Action<int> OnUpdateAction { get; private set; }

        /// <summary>
        /// 运行完毕
        /// </summary>
        public Action OnCompleteAction { get; private set; }

        /// <summary>
        /// 用于判断委托方是不是为null，如果为null则不调用委托， 避免报错
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        internal TimeAction Init(object target, float interval, int loop, Action<int> onUpdate, Action onComplete, bool unScaled)
        {
            if (tillTime > 0)
            {
                YouYou.GameEntry.LogError(LogCategory.Framework, "定时器正在使用中");
                return null;
            }
            Target = target;

            Unscaled = unScaled;

            tillTime = (Unscaled ? Time.unscaledTime : Time.time) + interval;

            m_Interval = interval;
            m_Loop = loop;
            OnUpdateAction = onUpdate;

            OnCompleteAction = onComplete;

            m_CurrLoop = 0;
            GameEntry.Time.Register(tillTime, this, Unscaled);
            return this;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            //防止重复Stop
            if (tillTime == 0) return;

            GameEntry.Time.Remove(tillTime, this, Unscaled);
            OnUpdateAction = null;
            OnCompleteAction = null;
            tillTime = 0;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            m_LastPauseTime = (Unscaled ? Time.unscaledTime : Time.time);
            GameEntry.Time.Remove(tillTime, this, Unscaled);
        }
        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            //计算暂停了多久
            tillTime += (Unscaled ? Time.unscaledTime : Time.time) - m_LastPauseTime;
            GameEntry.Time.Register(tillTime, this, Unscaled);
        }

        /// <summary>
        /// 时间到达
        /// </summary>
        public void TillTimeEnd()
        {
            //以下代码 间隔m_Interval 时间 执行一次
            if (Target == null)
            {
                GameEntry.LogWarning(LogCategory.Framework, "TimeAction.OnUpdateAction.Target==null");
                return;
            }
            m_CurrLoop++;
            OnUpdateAction?.Invoke(m_Loop - m_CurrLoop);
            if (m_CurrLoop >= m_Loop && m_Loop != -1)//-1表示无限次循环, 那么永远不会执行OnCompleteAction
            {
                if (Target == null)
                {
                    GameEntry.LogWarning(LogCategory.Framework, "TimeAction.OnCompleteAction.Target==null");
                    return;
                }
                //完成了，执行OnCompleteAction，结束循环
                OnCompleteAction?.Invoke();
            }
            else
            {
                //继续循环
                tillTime = (Unscaled ? Time.unscaledTime : Time.time) + m_Interval;
                GameEntry.Time.Register(tillTime, this, Unscaled);
            }
        }
    }
}
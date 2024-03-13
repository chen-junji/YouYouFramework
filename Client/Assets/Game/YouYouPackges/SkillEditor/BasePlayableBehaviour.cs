using Main;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYou
{
    /// <summary>
    /// Playable控制器基类
    /// </summary>
    /// <typeparam name="TP">Playable参数</typeparam>
    public abstract class BasePlayableBehaviour<TP> : PlayableBehaviour where TP : class, new()
    {
        /// <summary>
        /// 当前逻辑的参数
        /// </summary>
        public TP CurrArgs;

        /// <summary>
        /// 当前的技能控制器
        /// </summary>
        public TimelineCtrl CurrTimelineCtrl;

        /// <summary>
        /// 当前的PlayableDirector
        /// </summary>
        public PlayableDirector CurrPlayableDirector;

        public double Start = 0;
        public double End = 0;

        /// <summary>
        /// 未播放该clip？  false为未播放
        /// </summary>
        bool clipPlayed = false;


        /// <summary>
        /// 播放完该clip后是否需要暂停。false为不需要。
        /// 缓存的bool变量，会在时间轴运行到该clip时，根据 hasToPause 值赋值。
        /// </summary>
        bool pauseScheduled = false;

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            clipPlayed = false;
            pauseScheduled = false;
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
            if (Application.isPlaying)
            {
                //这里进行参数置空
                CurrArgs = null;
            }
        }

        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
            if (Application.isPlaying)
            {
                CurrPlayableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);

            // Debug.Log("OnBehaviourPlay==" + Application.isPlaying);
            if (Application.isPlaying)
            {
                //若还未播放该clip，当前clip的权重>0
                //效果是：时间轴刚进入该clip时执行一次
                if (!clipPlayed)
                {
                    OnYouYouBehaviourPlay(playable, info);
                    //检测播放完该clip后是否需要暂停，并赋值
                    pauseScheduled = true;
                    clipPlayed = true;
                }
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            //因为该回调会在刚运行TimeLine、执行完clip时都会执行一次。
            //因此不能直接判断 hasToPause ，否则刚运行TimeLine就给暂停了。

            if (Application.isPlaying)
            {
                if (pauseScheduled)
                {
                    if (CurrPlayableDirector == null)
                    {
                        OnYouYouBehaviourStop(playable, info);
                        return;
                    }

                    if (Mathf.Abs((float)CurrPlayableDirector.time - (float)End) < 0.2f ||
                        CurrPlayableDirector.time == 0)
                    {
                        pauseScheduled = false;
                        OnYouYouBehaviourStop(playable, info);
                    }
                }
            }
        }

        protected abstract void OnYouYouBehaviourPlay(Playable playable, FrameData info);
        protected abstract void OnYouYouBehaviourStop(Playable playable, FrameData info);
    }
}
using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace YouYou
{
    /// <summary>
    /// Playable视图基类
    /// </summary>
    /// <typeparam name="T">Playable控制器</typeparam>
    /// <typeparam name="TP">Playable参数</typeparam>
    public class BasePlayableAsset<T, TP> : PlayableAsset, ITimelineClipAsset
        where T : BasePlayableBehaviour<TP>, new()
        where TP : class, new()
    {
        /// <summary>
        /// 被允许的功能将可以在Inspector被编辑，这些功能都是与Clip相关的操作
        /// </summary>
        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        /// <summary>
        /// 当前视图对应的控制器
        /// </summary>
        public T CurrPlayableBehavior;

        [SerializeField] protected TP Args;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<T>.Create(graph);
            if (Application.isPlaying)
            {
                CurrPlayableBehavior = playable.GetBehaviour();
                if (CurrPlayableBehavior != null)
                {
                    //这里进行参数赋值
                    CurrPlayableBehavior.CurrArgs = Args;
                    OnYouYouCreatePlayable(playable);
                }
            }

            return playable;
        }

        protected virtual void OnYouYouCreatePlayable(ScriptPlayable<T> playable) { }
    }
}
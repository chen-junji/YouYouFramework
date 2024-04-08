using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYouFramework
{
    public class PlaySoundPlayable : BasePlayableAsset<PlaySoundPlayableBehaviour, PlaySoundEventArgs>
    {
    }
    public class PlaySoundPlayableBehaviour : BasePlayableBehaviour<PlaySoundEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.PlaySound?.Invoke(CurrArgs);
        }
        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
    [System.Serializable]
    public class PlaySoundEventArgs
    {
        /// <summary>
        /// 目标点
        /// </summary>
        [Header("目标点")]
        public DynamicTarget Target;

        /// <summary>
        /// 声音文件
        /// </summary>
        [Header("声音文件")]
        public AudioClip AudioClip;
    }
}
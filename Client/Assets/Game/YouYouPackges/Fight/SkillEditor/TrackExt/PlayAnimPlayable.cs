using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYou
{
    public class PlayAnimPlayable : BasePlayableAsset<PlayAnimPlayableBehaviour, PlayAnimEventArgs>
    {
    }
    public class PlayAnimPlayableBehaviour : BasePlayableBehaviour<PlayAnimEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.PlayAnim?.Invoke(CurrArgs);
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
    [System.Serializable]
    public class PlayAnimEventArgs
    {
        [Header("目标点")]
        public DynamicTarget Target;

        [Header("动画资源")]
        public AnimationClip AnimationClip;

        [Header("动画参数")]
        public int Param = 0;

    }
}
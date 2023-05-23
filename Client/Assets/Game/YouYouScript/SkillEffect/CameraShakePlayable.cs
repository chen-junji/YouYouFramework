using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYou
{
    public class CameraShakeEventArgs
    {
    }
    public class CameraShakePlayable : BasePlayableAsset<CameraShakePlayableBehaviour, CameraShakeEventArgs>
    {
    }
    public class CameraShakePlayableBehaviour : BasePlayableBehaviour<CameraShakeEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CameraCtrl.Instance.CameraShake();
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
}
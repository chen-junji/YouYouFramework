using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYouFramework
{
    public class CameraShakePlayable : BasePlayableAsset<CameraShakePlayableBehaviour, CameraShakeEventArgs>
    {
    }
    public class CameraShakePlayableBehaviour : BasePlayableBehaviour<CameraShakeEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.CameraShake?.Invoke(CurrArgs);
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
    public class CameraShakeEventArgs
    {
    }
}
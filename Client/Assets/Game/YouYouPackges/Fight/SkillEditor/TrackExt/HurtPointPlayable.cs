using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYouFramework
{
    public class HurtPointPlayable : BasePlayableAsset<HurtPointPlayableBehaviour, HurtPointEventArgs>
    {
    }
    public class HurtPointPlayableBehaviour : BasePlayableBehaviour<HurtPointEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.HurtPoint?.Invoke(CurrArgs);
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
    [System.Serializable]
    public class HurtPointEventArgs
    {
        [Header("…À∫¶∑∂Œß")]
        public int hurtRange = 5;

        [Header("…À∫¶÷µ")]
        public int hurtValue = 10;

        [Header("Buff¿‡±")]
        public BuffCategory buffCategory;

        [Header("Buff÷µ, ¿˝»Á—£‘Œ1.2√Î ‘ÚÃÓ1.2")]
        public float buffValue;
    }
}
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using YouYou;

namespace YouYou
{
    [System.Serializable]
    public class PlayResourceEventArgs
    {
#if UNITY_EDITOR
        [OnValueChanged("OnCurrResourceChanged")]
        public GameObject CurrResource;
        private void OnCurrResourceChanged()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(CurrResource);
            PrefabPath = path;
            PrefabName = CurrResource.name;
        }
#endif

        [Header("目标点")]
        public DynamicTarget Target;

        [Header("预设路径")]
        public string PrefabPath;

        [Header("预设名称")]
        public string PrefabName;

        [Header("偏移")]
        public Vector3 Offset;

        [Header("旋转")]
        public Vector3 Rotation;

        [Header("缩放")]
        public Vector3 Scale = Vector3.one;

    }
    public class PlayResourcePlayable : BasePlayableAsset<PlayResourcePlayableBehaviour, PlayResourceEventArgs>
    {
    }
    public class PlayResourcePlayableBehaviour : BasePlayableBehaviour<PlayResourceEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            if (CurrArgs.Target == DynamicTarget.OurOne)
            {
                PoolObj poolObj = GameEntry.Pool.GameObjectPool.Spawn(CurrArgs.PrefabName);
                poolObj.transform.SetParent(CurrTimelineCtrl.RoleCtrl.transform);
                poolObj.transform.localPosition = CurrArgs.Offset;
                poolObj.transform.localEulerAngles = CurrArgs.Rotation;
                poolObj.transform.localScale = CurrArgs.Scale;
                poolObj.SetDelayTimeDespawn((float)End);
            }
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
}
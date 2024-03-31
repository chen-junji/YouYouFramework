using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using YouYouFramework;


/// <summary>
/// 技能控制器
/// </summary>
public class TimelineCtrl : MonoBehaviour
{
    private PlayableDirector m_CurrPlayableDirector;

    //轨道剪辑触发 委托
    public Action<CameraShakeEventArgs> CameraShake;
    public Action<HurtPointEventArgs> HurtPoint;
    public Action<PlayAnimEventArgs> PlayAnim;
    public Action<PlayResourceEventArgs, float> PlayResource;
    public Action<PlaySoundEventArgs> PlaySound;

    /// <summary>
    /// 停止播放委托
    /// </summary>
    public Action OnStopped;

    /// <summary>
    /// 攻击结束时间
    /// </summary>
    public float AttackEndTime { get; private set; }

    private void OnDestroy()
    {
        m_CurrPlayableDirector.stopped -= OnPlayableDirectorStopped;
        m_CurrPlayableDirector = null;
    }
    private void Awake()
    {
        m_CurrPlayableDirector = GetComponent<PlayableDirector>();
        m_CurrPlayableDirector.stopped += OnPlayableDirectorStopped;
    }
    private void OnPlayableDirectorStopped(PlayableDirector playableDirector)
    {
        GameEntry.Pool.GameObjectPool.Despawn(transform);
        OnStopped?.Invoke();
    }
    private void OnEnable()
    {
        var tracks = m_CurrPlayableDirector.playableAsset.outputs.GetEnumerator();
        while (tracks.MoveNext())
        {
            PlayableBinding data = tracks.Current;
            if (data.sourceObject != null)
            {
                TrackAsset trackAsset = data.sourceObject as TrackAsset;
                if (trackAsset is PlaySoundTrack)
                {
                    Init<PlaySoundPlayableBehaviour, PlaySoundEventArgs>(trackAsset as PlaySoundTrack);
                }
                else if (trackAsset is PlayAnimTrack)
                {
                    PlayAnimTrack playAnimTrack = trackAsset as PlayAnimTrack;
                    Init<PlayAnimPlayableBehaviour, PlayAnimEventArgs>(playAnimTrack);
                    AttackEndTime = (float)playAnimTrack.end;
                }
                else if (trackAsset is PlayResourceTrack)
                {
                    Init<PlayResourcePlayableBehaviour, PlayResourceEventArgs>(trackAsset as PlayResourceTrack);
                }
                else if (trackAsset is HurtPointTrack)
                {
                    Init<HurtPointPlayableBehaviour, HurtPointEventArgs>(trackAsset as HurtPointTrack);
                }
                else if (trackAsset is CameraShakeTrack)
                {
                    Init<CameraShakePlayableBehaviour, CameraShakeEventArgs>(trackAsset as CameraShakeTrack);
                }
            }
        }
    }
    private void Init<T, TP>(TrackAsset trackAsset) where T : BasePlayableBehaviour<TP>, new() where TP : class, new()
    {
        var trackList = trackAsset.GetClips().GetEnumerator();
        while (trackList.MoveNext())
        {
            TimelineClip clip = trackList.Current;
            BasePlayableAsset<T, TP> playableAsset = clip.asset as BasePlayableAsset<T, TP>;
            if (playableAsset != null && playableAsset.CurrPlayableBehavior != null)
            {
                playableAsset.CurrPlayableBehavior.CurrTimelineCtrl = this;
                playableAsset.CurrPlayableBehavior.Start = clip.start;
                playableAsset.CurrPlayableBehavior.End = clip.end;
                playableAsset.CurrPlayableBehavior.Reset();
            }
        }
    }

}
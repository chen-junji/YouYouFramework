using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using YouYouFramework;

public class RoleAnimCompoent : MonoBehaviour
{
    [SerializeField] List<AnimationClip> firstClips;

    private Dictionary<string, RoleAnimInfo> m_RoleAnimInfoDic = new Dictionary<string, RoleAnimInfo>(m_AnimCount);
    private PlayableGraph m_PlayableGraph;
    private AnimationPlayableOutput m_AnimationPlayableOutput;
    private AnimationMixerPlayable m_AnimationMixerPlayable;
    private static int m_AnimCount = 100;//可以大于实际数量, 不能小于实际数量

    private Coroutine CoroutineDelayAnim;

    private float m_Mix;
    private float m_CurrentMixSpeed = 5;
    private int m_CurrAnimId;
    private int m_OldAnimId;

    private void Update()
    {
        //混合动画更新
        if (m_Mix == 0) return;
        m_Mix = Mathf.Max(0, m_Mix - Time.deltaTime * m_CurrentMixSpeed);

        //动画过度
        m_AnimationMixerPlayable.SetInputWeight(m_OldAnimId, m_Mix);
        m_AnimationMixerPlayable.SetInputWeight(m_CurrAnimId, 1 - m_Mix);
    }
    private void OnDestroy()
    {
        if (m_PlayableGraph.IsValid()) m_PlayableGraph.Destroy();
    }


    /// <summary>
    /// 初始化动画系统
    /// </summary>
    public void InitAnim(Animator animator)
    {
        //初始化Playable
        if (animator == null) return;
        if (m_PlayableGraph.IsValid()) m_PlayableGraph.Destroy();
        m_PlayableGraph = PlayableGraph.Create("PlayableGraph_" + animator.name);

        //创建混合Playable
        m_AnimationPlayableOutput = AnimationPlayableOutput.Create(m_PlayableGraph, "output", animator);
        m_AnimationMixerPlayable = AnimationMixerPlayable.Create(m_PlayableGraph, m_AnimCount);
        m_AnimationPlayableOutput.SetSourcePlayable(m_AnimationMixerPlayable, 0);

        //清空字典
        m_RoleAnimInfoDic.Clear();

        if (firstClips != null)
        {
            for (int i = 0; i < firstClips.Count; i++)
            {
                LoadAnimation(firstClips[i]);
            }
        }
    }


    public RoleAnimInfo PlayAnim(AnimationClip animClip, bool isLoop = false, Action onComplete = null)
    {
        return PlayAnim(animClip.name, isLoop, onComplete);
    }
    public RoleAnimInfo PlayAnim(string animName, bool isLoop = false, Action onComplete = null)
    {
        var enumerator = m_RoleAnimInfoDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            enumerator.Current.Value.IsPlaying = false;
        }

        if (m_RoleAnimInfoDic.TryGetValue(animName, out RoleAnimInfo roleAnimInfo))
        {
            roleAnimInfo.IsPlaying = true;
            PlayAnimByInputPort(roleAnimInfo, onComplete, isLoop);
        }
        else
        {
            onComplete?.Invoke();
        }
        return roleAnimInfo;
    }

    private void PlayAnimByInputPort(RoleAnimInfo roleAnimInfo, Action onComplete, bool isLoop)
    {
        m_PlayableGraph.Play();

        Playable playable = m_AnimationMixerPlayable.GetInput(roleAnimInfo.inputPort);
        playable.SetTime(0);
        playable.Play();

        //清空原有动画
        for (int i = 0; i < m_AnimCount; i++) m_AnimationMixerPlayable.SetInputWeight(i, 0);
        m_OldAnimId = m_CurrAnimId;
        m_CurrAnimId = roleAnimInfo.inputPort;
        if (m_CurrAnimId == m_OldAnimId)
        {
            m_AnimationMixerPlayable.SetInputWeight(m_CurrAnimId, 1);
        }
        else
        {
            m_AnimationMixerPlayable.SetInputWeight(m_OldAnimId, 1);
            m_Mix = 1;
        }

        IEnumerator DelayAnim()
        {
            yield return new WaitForSeconds(roleAnimInfo.CurrPlayable.GetAnimationClip().length);
            if (!isLoop) playable.Pause();
            onComplete?.Invoke();
        }
        if (CoroutineDelayAnim != null) StopCoroutine(CoroutineDelayAnim);
        CoroutineDelayAnim = StartCoroutine(DelayAnim());
    }

    /// <summary>
    /// 加载动画进入Playable集合
    /// </summary>
    public void LoadAnimation(AnimationClip animationClip)
    {
        //字典的长度作为动画索引
        int inputPort = m_RoleAnimInfoDic.Count;

        AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(m_PlayableGraph, animationClip);
        m_PlayableGraph.Connect(animationClipPlayable, 0, m_AnimationMixerPlayable, inputPort);
        m_AnimationMixerPlayable.SetInputWeight(inputPort, 0);
        m_RoleAnimInfoDic.Add(animationClip.name, new RoleAnimInfo()
        {
            inputPort = inputPort,
            CurrPlayable = animationClipPlayable,
        });
    }



    public class RoleAnimInfo
    {
        public int inputPort;
        public AnimationClipPlayable CurrPlayable;
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying;
    }
}
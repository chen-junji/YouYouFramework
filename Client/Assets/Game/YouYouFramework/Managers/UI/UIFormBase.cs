using UnityEngine;
using System.Threading.Tasks;
using YouYou;
using System.Collections.Generic;

public class UIFormBase : UIBase
{
    [Header("是否窗口动画")]
    [SerializeField] bool isAnim = false;

    [Header("UI特效分组")]
    [SerializeField] public List<UIEffectGroup> UIEffectGroups = new List<UIEffectGroup>();
    [Header("初始播放的特效")]
    [SerializeField] List<ParticleSystem> effectOnOpenPlay = new List<ParticleSystem>();

    protected override void OnEnable()
    {
        base.OnEnable();
#if UNITY_EDITOR
        transform.SetAsLastSibling();
#endif
        if (isAnim) AnimOpen();
    }
    protected override void Start()
    {
        base.Start();
        //根据UI层级, 设置特效层级
        for (int i = 0; i < UIEffectGroups.Count; i++)
        {
            UIEffectGroup effectGroup = UIEffectGroups[i];
            effectGroup.Group.ForEach(x =>
            {
                x.SetEffectOrder(CurrCanvas.sortingOrder + effectGroup.Order);
                x.gameObject.SetLayer("UI");
            });
        }

        //停止特效初始播放, 不需要可以注释
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++) particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        //播放设定的初始特效
        effectOnOpenPlay.ForEach(x => x.Play());
    }
    public void AnimOpen()
    {
        transform.DoShowScale(0.3f, 1);
    }
}
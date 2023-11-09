using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 主要用于解决特效无法渲染在UI上的问题
    /// </summary>
    [RequireComponent(typeof(Canvas))]//脚本依赖
    public class UIFormEffect : MonoBehaviour
    {
        [Header("是否窗口动画")]
        [SerializeField] bool isAnim = false;

        [Header("UI特效分组")]
        [SerializeField] public List<UIEffectGroup> UIEffectGroups = new List<UIEffectGroup>();
        [Header("初始播放的特效")]
        [SerializeField] List<ParticleSystem> effectOnOpenPlay = new List<ParticleSystem>();


        void Start()
        {
            Canvas CurrCanvas = GetComponent<Canvas>();
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
        private void OnEnable()
        {
#if UNITY_EDITOR
            transform.SetAsLastSibling();
#endif
            if (isAnim) AnimOpen();
        }

        public void AnimOpen()
        {
            transform.DoShowScale(0.3f, 1);
        }

    }

    /// <summary>
    /// UI特效分组
    /// </summary>
    [Serializable]
    public class UIEffectGroup
    {
        /// <summary>
        /// 排序
        /// </summary>
        public ushort Order;
        /// <summary>
        /// 特效组
        /// </summary>
        public List<Transform> Group = new List<Transform>();
    }
}
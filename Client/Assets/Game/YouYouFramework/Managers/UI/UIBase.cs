using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [RequireComponent(typeof(Canvas))]//脚本依赖
    [RequireComponent(typeof(GraphicRaycaster))]//脚本依赖
    public class UIBase : MonoBehaviour
    {
        [Header("UI特效分组")]
        [SerializeField] public List<UIEffectGroup> UIEffectGroups = new List<UIEffectGroup>();
        [Header("初始播放的特效")]
        [SerializeField] List<ParticleSystem> effectOnOpenPlay = new List<ParticleSystem>();

        public Sys_UIFormEntity SysUIForm { get; private set; }

        public Canvas CurrCanvas { get; private set; }

        public float CloseTime { get; private set; }

        //打开时调用
        public static Action ActionOpen;

        //反切时调用
        public Action OnBack;

        //是否活跃
        internal bool IsActive = true;


        protected virtual void Awake()
        {
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
            CurrCanvas = GetComponent<Canvas>();
        }
        protected virtual void OnEnable()
        {
        }
        protected virtual void Start()
        {
            GameEntry.Time.Yield(() =>
            {
                Button[] buttons = GetComponentsInChildren<Button>(true);
                for (int i = 0; i < buttons.Length; i++)
                {
                    Navigation navigation = buttons[i].navigation;
                    navigation.mode = Navigation.Mode.None;
                    buttons[i].navigation = navigation;
                }
            });
        }
        protected virtual void OnDisable()
        {

        }
        protected virtual void OnDestroy()
        {
        }

        public void Close()
        {
            GameEntry.UI.CloseUIForm(this);
        }

        internal void Init(Sys_UIFormEntity sysUIForm)
        {
            SysUIForm = sysUIForm;
        }
        internal void ToOpen()
        {
            //先设置UI层级
            if (SysUIForm.DisableUILayer != 1) GameEntry.UI.UILayer.SetSortingOrder(this, true);

            //再根据UI层级, 设置特效层级
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

            if (ActionOpen != null)
            {
                Action onOpenBegin = ActionOpen;
                ActionOpen = null;
                onOpenBegin();
            }
        }
        internal void ToClose()
        {
            //进行层级管理 减少层级
            if (SysUIForm.DisableUILayer != 1) GameEntry.UI.UILayer.SetSortingOrder(this, false);

            CloseTime = Time.time;
            GameEntry.UI.HideUI(this);
            GameEntry.UI.UIPool.EnQueue(this);
        }

    }
}
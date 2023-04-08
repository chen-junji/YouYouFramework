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
        /// <summary>
        /// 是否活跃
        /// </summary>
        internal bool IsActive = true;

        public Sys_UIFormEntity SysUIForm { get; private set; }

        /// <summary>
        /// 当前画布
        /// </summary>
        public Canvas CurrCanvas { get; private set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public float CloseTime { get; private set; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object UserData { get; private set; }

        [Header("UI特效分组")]
        [SerializeField] public List<UIEffectGroup> UIEffectGroups = new List<UIEffectGroup>();
        [Header("初始播放的特效")]
        [SerializeField] List<ParticleSystem> effectOnOpenPlay = new List<ParticleSystem>();

        public Action OnOpenBegin;

        //反切时调用
        public Action OnBack;

        //Start是否执行过? 为了避免预加载Start不执行,  首次打开又执行Start, 导致两次Open的问题
        public bool IsStart { get; private set; }


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
            OnInit(UserData);

            OnShow();//在没有调用Start之前是不能调用OnShow, 否则会出现OnInit没有初始化, OnShow方法没用的情况
            Open(UserData);
            IsStart = true;

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
        protected virtual void OnDestroy()
        {
            OnBeforDestroy();
        }

        internal void Init(Sys_UIFormEntity sysUIForm, object userData)
        {
            SysUIForm = sysUIForm;
            UserData = userData;
            OnLoadComplete();
        }

        internal void Open(object userData)
        {
            UserData = userData;

            //先设置UI层级
            if (SysUIForm != null && SysUIForm.DisableUILayer != 1) GameEntry.UI.UILayer.SetSortingOrder(this, true);
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

            if (OnOpenBegin != null)
            {
                Action onOpenBegin = OnOpenBegin;
                OnOpenBegin = null;
                onOpenBegin();
            }
            OnOpen(UserData);
        }

        public void Close()
        {
            GameEntry.UI.CloseUIForm(this);
        }
        internal void ToClose()
        {
            if (SysUIForm != null && SysUIForm.DisableUILayer != 1)
            {
                //进行层级管理 减少层级
                GameEntry.UI.UILayer.SetSortingOrder(this, false);
            }

            OnClose();

            CloseTime = Time.time;
            GameEntry.UI.HideUI(this);
            GameEntry.UI.UIPool.EnQueue(this);
        }

        //初始化, 最早调用, 克隆时调用一次
        protected virtual void OnInit(object userData) { }
        //打开, 第二调用, 克隆和对象池取出时调用(反切时不会调)
        protected virtual void OnOpen(object userData) { }
        //关闭, 对象回池时, 调用(反切时不会调)
        protected virtual void OnClose() { }
        //销毁, 对象池销毁该对象时, 调用一次
        protected virtual void OnBeforDestroy() { }
        //任何情况下 显示 都会调用
        internal virtual void OnShow() { }
        //任何情况下 隐藏 都会调用
        internal virtual void OnHide() { }
        //加载完毕后调用
        internal virtual void OnLoadComplete() { }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [RequireComponent(typeof(Canvas))]//脚本依赖
    [RequireComponent(typeof(GraphicRaycaster))]//脚本依赖
    public class UIFormBase : MonoBehaviour
    {
        public Sys_UIFormEntity SysUIForm { get; private set; }

        public Canvas CurrCanvas { get; private set; }

        public float CloseTime { get; private set; }

        //打开时调用
        public static Action ActionOpen;

        //反切时调用
        public Action OnBack;

        //是否活跃（防止有人自己改了gameObject.SetAcitve() 所以这里做了数值备份）
        internal bool IsActive = true;

        //当前canvas的sortingOrder（防止有人自己改了canvas.sortingOrder 所以这里做了数值备份）
        internal int sortingOrder = 0;


        protected virtual void Awake()
        {
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
            CurrCanvas = GetComponent<Canvas>();
        }
        protected virtual void Start()
        {
            GameEntry.Time.Yield(() =>
            {
                //这里是禁用所有按钮的导航功能，因为用不上, 还可能有意外BUG
                Button[] buttons = GetComponentsInChildren<Button>(true);
                for (int i = 0; i < buttons.Length; i++)
                {
                    Navigation navigation = buttons[i].navigation;
                    navigation.mode = Navigation.Mode.None;
                    buttons[i].navigation = navigation;
                }
            });
        }
        protected virtual void OnEnable()
        {

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
            //设置UI层级
            GameEntry.UI.SetSortingOrder(this, true);

            //UI打开时的委托
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
            GameEntry.UI.SetSortingOrder(this, false);

            CloseTime = Time.time;
            GameEntry.UI.HideUI(this);
            GameEntry.UI.EnQueue(this);
        }

        internal void SetSortingOrder(int sortingOrder)
        {
            if (SysUIForm.DisableUILayer == 1)
            {
                return;
            }
            this.sortingOrder = sortingOrder;
            CurrCanvas.overrideSorting = true;
            CurrCanvas.sortingOrder = sortingOrder;
        }

    }
}
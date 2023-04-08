using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    public static class GuideUtil
    {
        public static UIHollow UIHollow;
        public static void ShowOrNextHollow()
        {
            if (!UIHollow || !UIHollow.IsActive) GameEntry.UI.OpenUIForm(UIFormId.UIHollow, out UIHollow);

            if (UIHollow.GuideState != GameEntry.Guide.CurrentState)
            {
                UIHollow.SetUI(GameEntry.Guide.CurrentState);
            }
            else
            {
                UIHollow.NextGroup();
            }
        }
        public static void CloseHollow()
        {
            if (UIHollow && UIHollow.IsActive) UIHollow.Close();
        }

        //public static UIBubble UIBubble;
        //public static void ShowOrNextBubble()
        //{
        //	if (!UIBubble || !UIBubble.IsActive) UIBubble = GameEntry.UI.OpenUIForm<UIBubble>(UIFormId.UIBubble);

        //	if (UIBubble.GuideState != GameEntry.Guide.CurrentState)
        //	{
        //		UIBubble.OnOpenBegin = () =>
        //		{
        //			UIBubble.SetUI(GameEntry.Guide.CurrentState);
        //		};
        //	}
        //	else
        //	{
        //		UIBubble.NextGroup();
        //	}
        //}
        //public static void CloseBubble()
        //{
        //	if (UIBubble && UIBubble.IsActive) UIBubble.Close();
        //}


        /// <summary>
        /// 监听按钮点击, 触发下一步
        /// </summary>
        public static void CheckBtnNext(Button button, Action onNext = null)
        {
            button.onClick.AddListener(OnNext);
            GameEntry.Log(LogCategory.Hollow, "CheckBtnNext");
            void OnNext()
            {
                GameEntry.Log(LogCategory.Hollow, "CheckBtnNext-OnNext");
                button.onClick.RemoveListener(OnNext);

                onNext?.Invoke();
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
            }
        }
        /// <summary>
        /// 监听开关激活, 触发下一步
        /// </summary>
        public static void CheckToggleNext(Toggle toggle)
        {
            toggle.onValueChanged.AddListener(OnNext);
            void OnNext(bool isOn)
            {
                if (!isOn) return;
                toggle.onValueChanged.RemoveListener(OnNext);
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
            }
        }
        /// <summary>
        /// 监听事件, 触发下一步
        /// </summary>
        public static void CheckEventNext(string eventName)
        {
            GameEntry.Event.Common.AddEventListener(eventName, OnNext);
            void OnNext(object userData)
            {
                GameEntry.Event.Common.RemoveEventListener(eventName, OnNext);
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
            }
        }
        /// <summary>
        /// 监听窗口打开, 触发下一步
        /// </summary>
        public static void CheckUIOpenNext(UIBase ui)
        {
            ui.OnOpenBegin = () =>
            {
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
            };
        }
    }
}
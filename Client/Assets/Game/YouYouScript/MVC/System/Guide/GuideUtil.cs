using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYouFramework
{
    public static class GuideUtil
    {
        /// <summary>
        /// 监听按钮点击, 触发下一步
        /// </summary>
        public static void CheckBtnNext(Button button, Action onNext = null)
        {
            button.onClick.AddListener(OnNext);
            GameEntry.Log(LogCategory.Guide, "CheckBtnNext");
            void OnNext()
            {
                GameEntry.Log(LogCategory.Guide, "CheckBtnNext-OnNext");
                button.onClick.RemoveListener(OnNext);

                onNext?.Invoke();
                GuideCtrl.Instance.NextGroup(GuideCtrl.Instance.CurrentState);
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
                GuideCtrl.Instance.NextGroup(GuideCtrl.Instance.CurrentState);
            }
        }
        /// <summary>
        /// 监听事件, 触发下一步
        /// </summary>
        public static void CheckEventNext(GuideModel.GUIDE_ID eventName)
        {
            GameEntry.Model.GetModel<GuideModel>().AddEventListener((int)eventName, OnNext);
            void OnNext(object userData)
            {
                GameEntry.Model.GetModel<GuideModel>().RemoveEventListener((int)eventName, OnNext);
                GuideCtrl.Instance.NextGroup(GuideCtrl.Instance.CurrentState);
            }
        }
    }
}
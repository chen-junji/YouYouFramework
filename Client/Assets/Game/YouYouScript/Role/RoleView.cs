using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;


public class RoleView : MonoBehaviour
{
    /// <summary>
    /// 自定义动画组件
    /// </summary>
    public RoleAnimCompoent AnimCompoent { get; private set; }
    /// <summary>
    /// 自定义导航组件
    /// </summary>
    public RoleAgentCompoent Agent { get; private set; }

    ///// <summary>
    ///// 血条跟随点
    ///// </summary>
    //public Transform HeadBarPoint { get; private set; }

    ///// <summary>
    ///// 当前血条
    ///// </summary>
    //public UIGlobalHeadBarView HeadBarView { get; protected set; }

    ///// <summary>
    ///// 当前HUD
    ///// </summary>
    //[HideInInspector] public HUDText HUDText;


    protected virtual void OnDestroy()
    {
        //if (HeadBarView != null)
        //{
        //    GameEntry.Data.GlobalDataMgr.ReleaseHeadBarView(HeadBarView);
        //    HeadBarView = null;
        //}

        //if (HUDText != null)
        //{
        //    GameEntry.Data.GlobalDataMgr.ReleaseHudText(HUDText);
        //    HUDText = null;
        //}
    }
    protected virtual void Awake()
    {
        Agent = GetComponent<RoleAgentCompoent>();

        //初始化动画系统
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());

    }
    //protected virtual void Update()
    //{
    //    if (HeadBarPoint == null) return;
    //    //得到屏幕坐标
    //    Vector2 screenPos = CameraCtrl.Instance.MainCamera.WorldToScreenPoint(HeadBarPoint.position);

    //    //接收的UI世界坐标
    //    Vector3 pos;

    //    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(GameEntry.Instance.UIRootRectTransform, screenPos, GameEntry.Instance.UICamera, out pos))
    //    {
    //        HeadBarView.transform.position = pos;
    //        HUDText.transform.position = pos + new Vector3(0, 50, 0);
    //    }
    //}
}

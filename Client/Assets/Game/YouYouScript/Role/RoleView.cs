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

    protected virtual void OnDestroy()
    {
    }
    protected virtual void Awake()
    {
        Agent = GetComponent<RoleAgentCompoent>();

        //初始化动画系统
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());

    }
}

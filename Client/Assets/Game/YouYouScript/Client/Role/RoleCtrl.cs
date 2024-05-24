using System;
using UnityEngine;
using UnityEngine.AI;
using YouYouFramework;

public class RoleCtrl : MonoBehaviour
{
    /// <summary>
    /// 自定义导航组件
    /// </summary>
    public RoleAgentCompoent AgentCompoent { get; private set; }

    /// <summary>
    /// 自定义换装组件
    /// </summary>
    public RoleSkinComponent SkinComponent {  get; private set; }

    protected virtual void Awake()
    {
        AgentCompoent = GetComponent<RoleAgentCompoent>();
        SkinComponent = GetComponent<RoleSkinComponent>();
    }
}
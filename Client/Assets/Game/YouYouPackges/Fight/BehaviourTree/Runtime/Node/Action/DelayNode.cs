using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 延时节点
/// </summary>
[NodeInfo(Name = "内置/延时")]
public class DelayNode : BaseActionNode
{
    private static FieldInfo[] fieldInfos = typeof(DelayNode).GetFields(BindingFlags.Public | BindingFlags.Instance);

    /// <inheritdoc />
    public override FieldInfo[] FieldInfos => fieldInfos;

    /// <summary>
    /// 延时的时间
    /// </summary>
    [BBParamInfo(Name = "延时时间")]
    public BBParamFloat DelayTime;

    [NonSerialized]
    public float Timer;

    private Action<float> updateCallback;

    public DelayNode()
    {
        updateCallback = OnUpdate;
    }

    /// <inheritdoc />
    protected override void OnStart()
    {
        Timer = 0;
        UpdateManager.AddUpdateTimer(updateCallback);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        UpdateManager.RemoveUpdateTimer(updateCallback);
    }

    private void OnUpdate(float deltaTime)
    {
        Timer += deltaTime;
        if (Timer >= DelayTime.Value)
        {
            UpdateManager.RemoveUpdateTimer(updateCallback);
            Finish(true);
        }
    }

}
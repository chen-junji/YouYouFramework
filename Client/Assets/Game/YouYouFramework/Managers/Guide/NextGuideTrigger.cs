using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// 新手引导, 下一步触发器
/// </summary>
public class NextGuideTrigger : MonoBehaviour
{
    /// <summary>
    /// 什么名字进入后会触发
    /// </summary>
    public string[] triggerNames;

    public Action TriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        foreach (string name in triggerNames)
        {
            if (name == other.name)
            {
                //新手引导, 触发下一步
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);

                TriggerEnter?.Invoke();
            }
        }
    }
}

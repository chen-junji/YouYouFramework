using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;
public class NextGuideTrigger : MonoBehaviour
{
    //public string nextGuide;

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
                //GuideState state = nextGuide.ToEnum<GuideState>();
                //bool isNext = GameEntry.Guide.NextGroup(state);
                //if (!isNext)
                //{
                //    Debug.LogError("触发下一步失败, 检查枚举==" + state);
                //}

                //进行下一个操作
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);

                TriggerEnter?.Invoke();
            }
        }
    }


}

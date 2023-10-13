using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// 行为树调试器
/// </summary>
public static class BTDebugger
{
    /// <summary>
    /// 行为树实例名 -> 行为树实例
    /// </summary>
    public static Dictionary<string, BehaviourTree> BTInstanceDict = new Dictionary<string, BehaviourTree>();
        

    /// <summary>
    /// 添加行为树
    /// </summary>
    public static void Add(BehaviourTree bt)
    {
        if (BTInstanceDict.ContainsKey(bt.DebugName))
        {
            Debug.LogError($"BTDebugger重复添加行为树:{bt.DebugName}");
            return;
        }
        BTInstanceDict.Add(bt.DebugName,bt);
    }

    /// <summary>
    /// 移除行为树
    /// </summary>
    public static void Remove(BehaviourTree bt)
    {
        BTInstanceDict.Remove(bt.DebugName);
    }

    /// <summary>
    /// 获取行为树
    /// </summary>
    public static BehaviourTree Get(string debugName)
    {
        BTInstanceDict.TryGetValue(debugName, out var bt);
        return bt;
    }
}
#endif
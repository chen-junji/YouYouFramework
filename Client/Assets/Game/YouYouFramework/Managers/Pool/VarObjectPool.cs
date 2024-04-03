using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

/// <summary>
/// 变量对象池,  也是在类对象池取的变量对象, 只不过包了一层
/// </summary>
public class VarObjectPool
{
    /// <summary>
    /// 变量对象池锁
    /// </summary>
    private object m_VarObjectLock = new object();

#if UNITY_EDITOR
    /// <summary>
    /// 在监视面板显示的信息
    /// </summary>
    public Dictionary<Type, int> VarObjectInspectorDic = new Dictionary<Type, int>();
#endif

    /// <summary>
    /// 取出一个变量对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T DequeueVarObject<T>() where T : VariableBase, new()
    {
        lock (m_VarObjectLock)
        {
            T item = GameEntry.Pool.ClassObjectPool.Dequeue<T>();
#if UNITY_EDITOR
            Type t = item.GetType();
            if (VarObjectInspectorDic.ContainsKey(t))
            {
                VarObjectInspectorDic[t]++;
            }
            else
            {
                VarObjectInspectorDic[t] = 1;
            }
#endif
            return item;
        }
    }

    /// <summary>
    /// 变量对象回池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    public void EnqueueVarObject<T>(T item) where T : VariableBase
    {
        lock (m_VarObjectLock)
        {
            GameEntry.Pool.ClassObjectPool.Enqueue(item);
#if UNITY_EDITOR
            Type t = item.GetType();
            if (VarObjectInspectorDic.ContainsKey(t))
            {
                VarObjectInspectorDic[t]--;
                if (VarObjectInspectorDic[t] == 0)
                {
                    VarObjectInspectorDic.Remove(t);
                }
            }
#endif
        }
    }

}

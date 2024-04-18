using System;
using UnityEngine;

/// <summary>
/// 单例(查找或创建Gameobject)
/// </summary>
public class SingletonMonoInstansce<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type type = typeof(T);
                GameObject obj = new(type.Name, type);
                DontDestroyOnLoad(obj);
                obj.hideFlags = HideFlags.HideInHierarchy;
                instance = obj.GetComponent<T>();
            }
            return instance;
        }
    }
}
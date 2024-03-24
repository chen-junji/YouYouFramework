using UnityEngine;

/// <summary>
/// 单例(查找或创建Gameobject)
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoInstansce<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(obj);
                instance = obj.GetOrCreatComponent<T>();
            }
            return instance;
        }
    }
}
using UnityEngine;
using System.Collections;


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

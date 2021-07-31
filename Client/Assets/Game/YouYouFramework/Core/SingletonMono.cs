using UnityEngine;
using System.Collections;


/// <summary>
/// 单例(Mono)
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = (T)this;
        }
    }
    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

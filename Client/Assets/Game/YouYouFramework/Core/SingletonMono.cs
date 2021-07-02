using UnityEngine;
using System.Collections;


/// <summary>
/// 单例(Mono)
/// </summary>
public class SingletonMono<T> : MonoBehaviour
{
    public static T Instance { get; private set; }

    void Awake()
    {
        Instance = GetComponent<T>();

        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    void OnDestroy()
    {
        BeforeOnDestroy();
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void BeforeOnDestroy() { }
}

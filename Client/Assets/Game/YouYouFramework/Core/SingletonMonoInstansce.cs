using UnityEngine;
using System.Collections;


/// <summary>
/// ������(Mono)
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoInstansce<T> : MonoBehaviour where T : MonoBehaviour
{
    #region ����
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
    #endregion

    void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    void Destroy()
    {
        BeforeOnDestroy();
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    /// <summary>
    /// ����Destroy
    /// </summary>
    protected virtual void BeforeOnDestroy() { }
}

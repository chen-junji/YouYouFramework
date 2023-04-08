using UnityEngine;


/// <summary>
/// 单例(Mono)
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<T>(true);
            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = GetComponent<T>();
        }
    }
    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

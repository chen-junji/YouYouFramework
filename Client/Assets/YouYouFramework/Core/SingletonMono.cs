using UnityEngine;
using System.Collections;


/// <summary>
/// ������(Mono)
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour
{
	#region ����
	private static T instance;

	public static T Instance
	{
		get
		{
			return instance;
		}
	}
	#endregion

	void Awake()
	{
		instance = GetComponent<T>();

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
	/// <summary>
	/// ����Destroy
	/// </summary>
	protected virtual void BeforeOnDestroy() { }
}

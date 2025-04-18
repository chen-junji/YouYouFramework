using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// GameObject池, 对象定时自动回池
/// </summary>
public class GameObjectDespawnHandle : MonoBehaviour
{
    private Coroutine coroutine;
    private float DelayTimeDespawn;

    public Action OnDespawn;

    public void SetDelayTimeDespawn(float delayTime)
    {
        DelayTimeDespawn = delayTime;
        if (DelayTimeDespawn < 0)
        {
            GameEntry.LogError(LogCategory.Pool, "DelayTimeDespawn不允许小于0");
            return;
        }
        if (DelayTimeDespawn == 0)
        {
            GameEntry.Pool.GameObjectPool.Despawn(gameObject);
            return;
        }

        StopTime();
        coroutine = StartCoroutine(DelayDespawn());
    }

    private void StopTime()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator DelayDespawn()
    {
        yield return new WaitForSeconds(DelayTimeDespawn);
        OnDespawn?.Invoke();
        GameEntry.Pool.GameObjectPool.Despawn(gameObject);
    }

}

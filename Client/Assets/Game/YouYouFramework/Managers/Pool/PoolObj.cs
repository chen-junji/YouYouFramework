using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class PoolObj : MonoBehaviour
{
    public bool IsActive;

    [SerializeField] float DelayTimeDespawn;

    [HideInInspector] public bool IsNew;
    private TimeAction timeAction;

    public void SetDelayTimeDespawn(float delayTime)
    {
        if (DelayTimeDespawn > 0) return;//只能设置一次
        DelayTimeDespawn = delayTime;

        BeginTime();
    }

    public void BeginTime()
    {
        if (DelayTimeDespawn <= 0) return;

        //timeAction = GameEntry.Time.Create(delayTime: DelayTimeDespawn, onComplete: () =>
        //{
        //    GameEntry.Pool.GameObjectPool.Despawn(transform);
        //    timeAction = null;
        //});

        StartCoroutine(DelayDespawn());
    }
    public void StopTime()
    {
        IsActive = false;
        //if (timeAction != null)
        //{
        //    timeAction.Stop();
        //    timeAction = null;
        //}

        StopCoroutine(DelayDespawn());
    }

    IEnumerator DelayDespawn()
    {
        yield return new WaitForSeconds(DelayTimeDespawn);
        GameEntry.Pool.GameObjectPool.Despawn(transform);
    }

    public void Despawn()
    {
        GameEntry.Pool.GameObjectPool.Despawn(this);
    }
}

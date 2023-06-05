using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class PoolObj : MonoBehaviour
{
    public bool IsActive;

    [SerializeField] float DelayTimeDespawn;

    [HideInInspector] public bool IsNew;

    private List<PoolObj> ChildObj = new List<PoolObj>();

    public Action OnDespawn;

    protected virtual void OnDestory() { }
    protected virtual void Start()
    {
        if (IsNew)
        {
            OnInit();
            OnOpen();
        }
    }
    public virtual void OnInit() { }
    public virtual void OnOpen() { }
    public virtual void OnClose()
    {
        for (int i = 0; i < ChildObj.Count; i++)
        {
            ChildObj[i].Despawn();
        }
        ChildObj.Clear();
    }

    public void AddChild(PoolObj poolObj)
    {
        if (poolObj == null) return;
        ChildObj.Add(poolObj);
    }

    public void Despawn()
    {
        OnDespawn?.Invoke();
        GameEntry.Pool.GameObjectPool.Despawn(this);
    }

    public void SetDelayTimeDespawn(float delayTime)
    {
        if (DelayTimeDespawn > 0) return;//只能设置一次
        DelayTimeDespawn = delayTime;

        BeginTime();
    }

    public void BeginTime()
    {
        if (DelayTimeDespawn <= 0) return;
        StartCoroutine(DelayDespawn());
    }
    public void StopTime()
    {
        IsActive = false;
        StopCoroutine(DelayDespawn());
    }

    IEnumerator DelayDespawn()
    {
        yield return new WaitForSeconds(DelayTimeDespawn);
        Despawn();
    }

}

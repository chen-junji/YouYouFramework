using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class ILRuntimeForm : UIFormBase
{
    public Action<object> onInit;
    public Action<object> onOpen;
    public Action onClose;
    public Action onBeforDestroy;
    public Action onUpdate;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        if (ILRuntimeManager.m_AppDomain == null) return;

        string className = string.Format("Hotfix.{0}", gameObject.name.Replace(" (Clone)", ""));
        object obj = ILRuntimeManager.m_AppDomain.Instantiate(className);
        ILRuntimeManager.m_AppDomain.Invoke(className, "Init", obj, this);

        onInit?.Invoke(userData);
    }
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        onOpen?.Invoke(userData);
    }
    protected override void OnClose()
    {
        base.OnClose();
        onClose?.Invoke();
    }
    protected override void OnBeforDestroy()
    {
        base.OnBeforDestroy();
        onBeforDestroy?.Invoke();

        onInit = null;
        onOpen = null;
        onClose = null;
        onBeforDestroy = null;
    }
    private void Update()
    {
        onUpdate?.Invoke();
    }
}

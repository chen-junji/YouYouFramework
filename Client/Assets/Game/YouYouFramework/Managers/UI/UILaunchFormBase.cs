using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UILaunchFormBase : MonoBehaviour
{
    public Canvas CurrCanvas { get; private set; }

    private bool m_IsInit;

    void Awake()
    {
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        CurrCanvas = GetComponent<Canvas>();
    }
    void OnDestroy()
    {
        OnBeforDestroy();
    }

    public void Open()
    {
        if (gameObject.activeInHierarchy) return;

        if (!m_IsInit)
        {
            OnInit();
            m_IsInit = true;
        }
        gameObject.SetActive(true);
        OnOpen();
    }
    public void Close()
    {
        if (!gameObject.activeInHierarchy) return;
        gameObject.SetActive(false);
        OnClose();
    }

    protected virtual void OnInit() { }
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
    protected virtual void OnBeforDestroy() { }
}

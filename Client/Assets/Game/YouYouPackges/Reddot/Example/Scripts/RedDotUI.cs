using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YouYouFramework;

public class RedDotUI : MonoBehaviour,IPointerClickHandler
{
    public string Path;

    [Header("控制的红点对象")]
    public GameObject imageRed;

    [Header("红点计数，用于测试展示")]
    private Text txt;


    private void Awake()
    {
        txt = GetComponentInChildren<Text>();

        //先初始化设置为不可见，避免闪一下
        SetRedDotVisible(false);
    }
    void Start()
    {
        TreeNode node = ReddotManager.Instance.AddListener(Path, ReddotCallback);
        gameObject.name = node.FullPath;
    }

    private void ReddotCallback(int value)
    {
        Debug.Log("红点刷新，路径:" + Path + ",当前帧数:" + Time.frameCount + ",值:" + value);
        txt.text = value.ToString();
        SetRedDotVisible(value > 0);
    }

    public void SetRedDotVisible(bool visible)
    {
        if (imageRed != null)
        {
            imageRed.gameObject.SetActive(visible);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int value = ReddotManager.Instance.GetValue(Path);

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ReddotManager.Instance.ChangeValue(Path, value + 1);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ReddotManager.Instance.ChangeValue(Path, Mathf.Clamp(value - 1,0, value));
        }
    }
}

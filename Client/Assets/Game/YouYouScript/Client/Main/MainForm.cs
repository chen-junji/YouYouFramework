using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;
using System;

public partial class MainForm : UIFormBase
{
    [SerializeField] private RectTransform m_Trans_BtnGroup;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEntry.Model.GetModel<MainModel>().TestAction -= OnTest;
    }
    protected override void Awake()
    {
        base.Awake();
        foreach (Transform child in m_Trans_BtnGroup)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    GameEntry.UI.CloseAllDefaultUIForm();
                    GameEntry.UI.OpenUIForm<LoadingForm>();
                    GameEntry.Scene.LoadSceneAction(button.GetComponentInChildren<Text>().text);
                });
            }
        }

        //演示MVC 代码范例
        GameEntry.Model.GetModel<MainModel>().TestAction += OnTest;
        MainCtrl.Instance.SendTest();
    }

    private void OnTest(int obj)
    {
        //假装这是后端给的数据 可以贴到UI上
        Debug.Log("这个Log文本可以随便修改 测试热更新222");
    }

}

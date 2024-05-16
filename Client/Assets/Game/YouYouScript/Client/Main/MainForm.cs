using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

public partial class MainForm : UIFormBase
{
    [SerializeField] private RectTransform m_Trans_BtnGroup;

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
    }
}

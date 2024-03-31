using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;


/// <summary>
/// 提示窗口
/// </summary>
public partial class DialogForm : UIFormBase
{
    private Action m_OkAction;

    private Action m_CancelAction;


    protected override void Awake()
    {
        base.Awake();
        m_Btn_OK.onClick.AddListener(() =>
        {
            m_OkAction?.Invoke();
            Close();
        });
        m_Btn_Cancel.onClick.AddListener(() =>
        {
            m_CancelAction?.Invoke();
            Close();
        });
    }

    public static void ShowForm(string message = "", string title = "提示", DialogFormType type = DialogFormType.Affirm, Action okAction = null, Action cancelAction = null)
    {
        DialogForm formDialog = GameEntry.UI.OpenUIForm<DialogForm>();
        formDialog.SetUI(message, title, type, okAction, cancelAction);
    }

    private void SetUI(string message = "", string title = "提示", DialogFormType type = DialogFormType.Affirm, Action okAction = null, Action cancelAction = null)
    {
        //窗口内容
        m_Txt_Title.text = title;
        m_Txt_Message.text = message;

        //点击按钮的类型
        switch (type)
        {
            case DialogFormType.Affirm:
                m_Btn_OK.gameObject.SetActive(true);
                m_Btn_Cancel.gameObject.SetActive(false);
                break;
            case DialogFormType.AffirmAndCancel:
                m_Btn_Cancel.gameObject.SetActive(true);
                m_Btn_OK.gameObject.SetActive(true);
                break;
        }

        m_OkAction = okAction;
        m_CancelAction = cancelAction;
    }
}

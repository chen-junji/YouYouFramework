using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// 提示窗口
/// </summary>
public class FormDialog : UIFormBase
{
    /// <summary>
    /// 标题
    /// </summary>
    [SerializeField]
    private Text lblTitle;

    /// <summary>
    /// 内容
    /// </summary>
    [SerializeField]
    private Text lblMessage;

    /// <summary>
    /// 确定按钮
    /// </summary>
    [SerializeField]
    private Button btnOK;

    /// <summary>
    /// 取消按钮
    /// </summary>
    [SerializeField]
    private Button btnCancel;

    private Action m_OkAction;

    private Action m_CancelAction;


    protected override void Awake()
    {
        base.Awake();
        btnOK.onClick.AddListener(() =>
        {
            m_OkAction?.Invoke();
            Close();
        });
        btnCancel.onClick.AddListener(() =>
        {
            m_CancelAction?.Invoke();
            Close();
        });
    }

    public static void ShowForm(string message = "", string title = "提示", DialogFormType type = DialogFormType.Affirm, Action okAction = null, Action cancelAction = null)
    {
        FormDialog formDialog = GameEntry.UI.OpenUIForm<FormDialog>();
        formDialog.SetUI(message, title, type, okAction, cancelAction);
    }

    public void SetUI(string message = "", string title = "提示", DialogFormType type = DialogFormType.Affirm, Action okAction = null, Action cancelAction = null)
    {
        //窗口内容
        lblTitle.text = title;
        lblMessage.text = message;

        //点击按钮的类型
        switch (type)
        {
            case DialogFormType.Affirm:
                btnOK.gameObject.SetActive(true);
                btnCancel.gameObject.SetActive(false);
                break;
            case DialogFormType.AffirmAndCancel:
                btnCancel.gameObject.SetActive(true);
                btnOK.gameObject.SetActive(true);
                break;
        }

        m_OkAction = okAction;
        m_CancelAction = cancelAction;
    }
}

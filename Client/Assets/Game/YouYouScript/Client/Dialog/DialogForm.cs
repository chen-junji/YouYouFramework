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
    /// <summary>
    /// 提示窗口,按钮显示方式
    /// </summary>
    public enum DialogFormType
    {
        /// <summary>
        /// 确定按钮
        /// </summary>
        Affirm,
        /// <summary>
        /// 确定,取消按钮
        /// </summary>
        AffirmAndCancel
    }

    [SerializeField] Text textTitle;
    [SerializeField] Text textContent;
    [SerializeField] Button btnOK;
    [SerializeField] Button btnCancel;
    [SerializeField] Text textOK;
    [SerializeField] Text textCancel;

    private Action actionOK;

    private Action actionCancel;


    protected override void Awake()
    {
        base.Awake();
        btnOK.onClick.AddListener(() =>
        {
            actionOK?.Invoke();
            this.Close();
        });
        btnCancel.onClick.AddListener(() =>
        {
            actionCancel?.Invoke();
            this.Close();
        });
    }

    public static void ShowFormByKey(string key, DialogFormType type = DialogFormType.Affirm, Action okAction = null, Action cancelAction = null)
    {
        if (GameEntry.DataTable.Sys_DialogDBModel.keyDic.TryGetValue(key, out var entity))
        {
            ShowForm(entity.Content, entity.Title, entity.BtnText1, entity.BtnText2, type, okAction, cancelAction);
        }
        else
        {
            GameEntry.LogError("当前Key找不到对应的表格配置, Key==" + key);
        }
    }
    public static async void ShowForm(string contenct = "", string title = "提示", string textBtn1 = "确定", string textBtn2 = "取消", DialogFormType type = DialogFormType.AffirmAndCancel, Action okAction = null, Action cancelAction = null)
    {
        DialogForm formDialog = await GameEntry.UI.OpenUIForm<DialogForm>();
        formDialog.SetUI(contenct, title, textBtn1, textBtn2, type, okAction, cancelAction);
    }

    private void SetUI(string contenct = "", string title = "提示", string textBtn1 = "确定", string textBtn2 = "取消", DialogFormType type = DialogFormType.AffirmAndCancel, Action okAction = null, Action cancelAction = null)
    {
        //窗口内容
        textTitle.text = title;
        textContent.text = contenct;
        textOK.text = textBtn1;
        textCancel.text = textBtn2;

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

        actionOK = okAction;
        actionCancel = cancelAction;
    }
}

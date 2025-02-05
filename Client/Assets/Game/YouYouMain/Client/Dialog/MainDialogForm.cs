using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//为什么要搞2个DialogForm?  因为1个要热更(可以读表, 不能出现在检查更新界面) 1个不要热更(不能读表, 可以出现在检查更新界面)
public class MainDialogForm : MonoBehaviour
{
    private static MainDialogForm Instance;

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


    protected void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        btnOK.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            actionOK?.Invoke();
        });
        btnCancel.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            actionCancel?.Invoke();
        });
    }

    public static void ShowForm(string contenct = "", string title = "提示", string textBtn1 = "确定", string textBtn2 = "取消", DialogFormType type = DialogFormType.AffirmAndCancel, Action okAction = null, Action cancelAction = null)
    {
        Instance.gameObject.SetActive(true);
        Instance.SetUI(contenct, title, textBtn1, textBtn2, type, okAction, cancelAction);
    }

    private void SetUI(string contenct = "", string title = "提示", string textBtn1 = "确定", string textBtn2 = "取消", DialogFormType type = DialogFormType.AffirmAndCancel, Action okAction = null, Action cancelAction = null)
    {
        gameObject.SetActive(true);

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

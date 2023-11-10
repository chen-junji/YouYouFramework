using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

/// <summary>
/// 镂空遮罩，只有镂空区域可点击（button不在这个界面，而在实际的业务场景界面）
/// </summary>
[RequireComponent(typeof(HollowOutMask))]
public class FormHollow : UIFormBase
{
    public static void ShowDialog(string formName)
    {
        GameEntry.UI.OpenUIForm<FormHollow>(formName);
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<HollowOutMask>().IsAcross = true;
    }
}

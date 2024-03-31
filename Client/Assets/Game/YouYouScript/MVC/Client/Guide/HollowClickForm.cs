using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

/// <summary>
/// 镂空遮罩，只有镂空区域可点击（button不在这个界面，而在实际的业务场景界面）
/// </summary>
[RequireComponent(typeof(HollowOutMask))]
public class HollowClickForm : MonoBehaviour
{
    public static GameObject ShowDialog(string formName)
    {
        return GameUtil.LoadPrefabClone(formName);
    }
    private void Start()
    {
        GetComponent<HollowOutMask>().IsAcross = true;
    }
}

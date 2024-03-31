using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

/// <summary>
/// 镂空遮罩，全屏可点击
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(HollowOutMask))]
public class HollowDialogForm : MonoBehaviour
{
    [Header("强制观看时间")]
    [SerializeField] float DelayTime;

    private Button button;

    public static void ShowDialog(string formName)
    {
        GameUtil.LoadPrefabClone(formName);
    }
    private void Start()
    {
        GetComponent<HollowOutMask>().IsAcross = false;

        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            //关闭自己
            Destroy(gameObject);

            //进行下一个操作
            GuideCtrl.Instance.NextGroup(GuideCtrl.Instance.CurrentState);
        });
    }
    private void OnEnable()
    {
        //强制玩家看一会儿
        if (DelayTime > 0)
        {
            button.enabled = false;
            GameEntry.Time.CreateTimer(this, DelayTime, () =>
            {
                button.enabled = true;
            });
        }
    }
}

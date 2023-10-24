using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


[RequireComponent(typeof(Button))]
public class HollowOutMaskBtn : HollowOutMask
{
    private Button button;

    [Header("强制观看时间")]
    [SerializeField] float DelayTime;

    protected override void Awake()
    {
        base.Awake();
        IsAcross = false;
        button = GetComponent<Button>();
        button.targetGraphic = this;
        button.onClick.AddListener(() =>
        {
            //进行下一个操作
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
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

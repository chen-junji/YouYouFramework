using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// 游戏主流程
/// </summary>
public class ProcedureMain : ProcedureBase
{
    internal override void OnEnter()
    {
        base.OnEnter();
        GameEntry.UI.OpenUIForm<MainForm>();
    }
    internal override void OnLeave()
    {
        base.OnLeave();

        //退出登录时, 清空业务数据
        GameEntry.Model.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;


/// <summary>
/// 登录流程
/// </summary>
public class ProcedureLogin : ProcedureBase
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        GameEntry.Procedure.ChangeState(ProcedureManager.EState.Main);


    }
    internal override void OnDestroy()
    {
        base.OnDestroy();
    }
}
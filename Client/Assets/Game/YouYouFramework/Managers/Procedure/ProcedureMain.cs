using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

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
        GameEntry.UI.CloseAllDefaultUIForm();
    }
}

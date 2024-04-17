using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYouFramework
{
    /// <summary>
    /// 登录流程
    /// </summary>
    public class ProcedureLogin : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Procedure.ChangeState(ProcedureState.Main);
        }
        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
        internal override void OnLeave()
        {
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
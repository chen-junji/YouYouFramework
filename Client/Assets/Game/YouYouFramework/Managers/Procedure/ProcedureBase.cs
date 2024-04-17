using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 流程状态基类
    /// </summary>
    public class ProcedureBase : FsmState<ProcedureManager>
    {
		internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnEnter()");
        }

		internal override void OnUpdate()
        {
            base.OnUpdate();
        }

		internal override void OnLeave()
        {
            base.OnLeave();
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnLeave()");
        }

		internal override void OnDestroy()
        {
            base.OnDestroy();

        }
    }
}
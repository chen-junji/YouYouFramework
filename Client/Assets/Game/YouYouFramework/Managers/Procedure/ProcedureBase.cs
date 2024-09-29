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
        public override void OnEnter(int lastState)
        {
            base.OnEnter(lastState);
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>OnEnter()");
        }

        public override void OnLeave(int newState)
        {
            base.OnLeave(newState);
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>OnLeave()");
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

        }
    }
}
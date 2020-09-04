using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
    /// <summary>
    /// 流程状态基类
    /// </summary>
    public class ProcedureBase : FsmState<ProcedureManager>
    {
        public override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnEnter()");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnLeave()
        {
            base.OnLeave();
            GameEntry.Log(LogCategory.Procedure, CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnLeave()");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

        }
    }
}
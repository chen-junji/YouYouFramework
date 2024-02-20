using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// µÇÂ¼Á÷³Ì
    /// </summary>
    public class ProcedureLogin : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Procedure.ChangeState(ProcedureState.Game);
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
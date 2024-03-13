using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureGame : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
        }
        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
        internal override void OnLeave()
        {
            base.OnLeave();

            //退出登录时, 清空业务数据
            GameEntry.Model.Clear();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
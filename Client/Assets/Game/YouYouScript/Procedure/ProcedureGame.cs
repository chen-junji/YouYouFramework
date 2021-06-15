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
			GameEntry.ILRuntime.AppDomain.Invoke("Hotfix.GameEntryIL", "ProcedureGameOnEnter", null);
			//GameEntry.UI.OpenDialogForm("框架内部流程全部加载完毕, 已经进入登录流程", "登录流程");
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
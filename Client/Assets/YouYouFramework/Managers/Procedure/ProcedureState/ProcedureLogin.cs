using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
	/// <summary>
	/// 登录流程-处理登录的业务逻辑
	/// </summary>
	public class ProcedureLogin : ProcedureBase
	{
		public override void OnEnter()
		{
			base.OnEnter();
			GameEntry.UI.OpenDialogForm("框架内部流程全部加载完毕, 可以进入到业务流程了~(假装自己是登录界面)", "登录");

		}
		public override void OnUpdate()
		{
			base.OnUpdate();
		}
		public override void OnLeave()
		{
			base.OnLeave();
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}
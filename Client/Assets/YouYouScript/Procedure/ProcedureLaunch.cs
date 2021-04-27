using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// 启动流程
	/// </summary>
	public class ProcedureLaunch : ProcedureBase
	{
		internal override void OnEnter()
		{
			base.OnEnter();
#if RESOURCES || EDITORLOAD
			GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#elif ASSETBUNDLE
			//访问账号服务器
			string url = GameEntry.Http.RealWebAccountUrl + "/init";
			Dictionary<string, object> dic = GameEntry.Pool.DequeueClassObject<Dictionary<string, object>>();
			dic.Clear();
			dic["ChannelId"] = GameEntry.Data.SysDataManager.CurrChannelConfig.ChannelId;
			dic["InnerVersion"] = GameEntry.Data.SysDataManager.CurrChannelConfig.InnerVersion;
			GameEntry.Http.Post(url, dic.ToJson(), false, (retJson) =>
			{
				if (!retJson.JsonCutApart<bool>("HasError"))
				{
					string config = retJson.JsonCutApart("Value");
					GameEntry.Data.SysDataManager.CurrChannelConfig.ServerTime = config.JsonCutApart<long>("ServerTime");
					GameEntry.Data.SysDataManager.CurrChannelConfig.SourceVersion = config.JsonCutApart("SourceVersion");
					GameEntry.Data.SysDataManager.CurrChannelConfig.SourceUrl = config.JsonCutApart("SourceUrl");
					GameEntry.Data.SysDataManager.CurrChannelConfig.TDAppId = config.JsonCutApart("TDAppId");
					GameEntry.Data.SysDataManager.CurrChannelConfig.IsOpenTD = config.JsonCutApart<bool>("IsOpenTD");
					GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
				}
			});
#endif
		}
	}
}
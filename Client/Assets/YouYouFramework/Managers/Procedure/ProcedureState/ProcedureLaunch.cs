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
#if RESOURCES
			GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#else
			GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);

			////访问账号服务器
			//string url = GameEntry.Http.RealWebAccountUrl + "/init";

			//Dictionary<string, object> dic = GameEntry.Pool.DequeueClassObject<Dictionary<string, object>>();
			//dic.Clear();

			//GameEntry.Data.SysDataManager.CurrChannelConfig.ChannelId = 0;
			//GameEntry.Data.SysDataManager.CurrChannelConfig.InnerVersion = 1001;

			//dic["ChannelId"] = GameEntry.Data.SysDataManager.CurrChannelConfig.ChannelId;
			//dic["InnerVersion"] = GameEntry.Data.SysDataManager.CurrChannelConfig.InnerVersion;

			//GameEntry.Http.SendData(url, (HttpCallBackArgs args) =>
			//{
			//	if (!args.HasError)
			//	{
			//		RetValue retValue = JsonMapper.ToObject<RetValue>(args.Value);
			//		if (!retValue.HasError)
			//		{
			//			Dictionary<string, object> config = JsonMapper.ToObject<Dictionary<string, object>>(retValue.Value.ToString());

			//			GameEntry.Data.SysDataManager.CurrChannelConfig.ServerTime = long.Parse(config["ServerTime"].ToString());
			//			GameEntry.Data.SysDataManager.CurrChannelConfig.SourceVersion = config["SourceVersion"].ToString();
			//			GameEntry.Data.SysDataManager.CurrChannelConfig.SourceUrl = config["SourceUrl"].ToString();
			//			GameEntry.Data.SysDataManager.CurrChannelConfig.RechargeUrl = config["RechargeUrl"].ToString();
			//			GameEntry.Data.SysDataManager.CurrChannelConfig.TDAppId = config["TDAppId"].ToString();
			//			GameEntry.Data.SysDataManager.CurrChannelConfig.IsOpenTD = bool.Parse(config["IsOpenTD"].ToString());

			//			GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
			//		}
			//	}
			//}, dic: dic);
#endif

		}
	}
}
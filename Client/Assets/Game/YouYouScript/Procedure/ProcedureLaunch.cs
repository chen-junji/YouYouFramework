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
			var value = new
            {
                ChannelId = GameEntry.Data.SysDataManager.CurrChannelConfig.ChannelId,
                InnerVersion = GameEntry.Data.SysDataManager.CurrChannelConfig.InnerVersion,
            };
			GameEntry.Http.Post(GameEntry.Http.RealWebAccountUrl + "/init", value.ToJson(), false, (retJson) =>
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
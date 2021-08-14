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
            //var value = new
            //{
            //    ChannelId = GameEntry.Data.SysData.CurrChannelConfig.ChannelId,
            //    InnerVersion = GameEntry.Data.SysData.CurrChannelConfig.InnerVersion,
            //};
            //GameEntry.Http.Post(GameEntry.Http.RealWebAccountUrl + "/init", value.ToJson(), false, (retJson) =>
            //{
            //    if (!retJson.JsonCutApart<bool>("HasError"))
            //    {
            //        string config = retJson.JsonCutApart("Value");
            //        GameEntry.Data.SysData.CurrChannelConfig.ServerTime = config.JsonCutApart<long>("ServerTime");
            //        GameEntry.Data.SysData.CurrChannelConfig.SourceVersion = config.JsonCutApart("SourceVersion");
            //        GameEntry.Data.SysData.CurrChannelConfig.SourceUrl = config.JsonCutApart("SourceUrl");
            //        GameEntry.Data.SysData.CurrChannelConfig.TDAppId = config.JsonCutApart("TDAppId");
            //        GameEntry.Data.SysData.CurrChannelConfig.IsOpenTD = config.JsonCutApart<bool>("IsOpenTD");
            //        GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
            //    }
            //});
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#endif
        }
    }
}
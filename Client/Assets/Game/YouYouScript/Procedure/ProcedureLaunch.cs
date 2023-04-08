using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace YouYou
{
    /// <summary>
    /// 启动流程
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        private string[] permissions = new string[]
        {
            "android.permission.WRITE_EXTERNAL_STORAGE"
        };
        internal override void OnEnter()
        {
            base.OnEnter();
            //初始画质设置
            GameEntry.Quality.SetQuality((QualityManager.Quality)GameEntry.PlayerPrefs.GetInt(CommonEventId.QualityLevel));
            GameEntry.Quality.SetScreen((QualityManager.ScreenLevel)GameEntry.PlayerPrefs.GetInt(CommonEventId.Screen));
            GameEntry.Quality.SetFrameRate((QualityManager.FrameRate)GameEntry.PlayerPrefs.GetInt(CommonEventId.FrameRate));

            //获取安卓权限
            permissions.ToList().ForEach(s =>
            {
                //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });

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
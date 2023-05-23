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
            //初始化配置代码
            SceneConst sceneConst = new SceneConst();

            //初始画质设置
            GameEntry.Quality.SetQuality((QualityManager.Quality)GameEntry.Data.PlayerPrefsDataMgr.GetInt(PlayerPrefsDataMgr.EventName.QualityLevel));
            GameEntry.Quality.SetScreen((QualityManager.ScreenLevel)GameEntry.Data.PlayerPrefsDataMgr.GetInt(PlayerPrefsDataMgr.EventName.Screen));
            GameEntry.Quality.SetFrameRate((QualityManager.FrameRate)GameEntry.Data.PlayerPrefsDataMgr.GetInt(PlayerPrefsDataMgr.EventName.FrameRate));

            //获取安卓权限
            permissions.ToList().ForEach(s =>
            {
                //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });

#if RESOURCES || EDITORLOAD
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#elif ASSETBUNDLE
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#endif
        }
    }
}
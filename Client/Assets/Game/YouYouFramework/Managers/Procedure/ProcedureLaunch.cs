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
            QualityCtrl.Instance.SetQuality((QualityCtrl.Quality)GameEntry.PlayerPrefs.GetInt(PlayerPrefsDataMgr.EventName.QualityLevel));
            QualityCtrl.Instance.SetScreen((QualityCtrl.ScreenLevel)GameEntry.PlayerPrefs.GetInt(PlayerPrefsDataMgr.EventName.Screen));
            QualityCtrl.Instance.SetFrameRate((QualityCtrl.FrameRate)GameEntry.PlayerPrefs.GetInt(PlayerPrefsDataMgr.EventName.FrameRate));

            //获取安卓权限
            permissions.ToList().ForEach(s =>
            {
                //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });

#if EDITORLOAD
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#elif ASSETBUNDLE
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#endif
        }
    }
}
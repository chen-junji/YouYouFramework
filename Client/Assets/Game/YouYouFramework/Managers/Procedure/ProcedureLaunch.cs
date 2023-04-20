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
            AudioConst audioConst = new AudioConst();
            PrefabConst prefabConst = new PrefabConst();
            SceneConst sceneConst = new SceneConst();

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
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#endif
        }
    }
}
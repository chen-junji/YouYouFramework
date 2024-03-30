using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 检查更新流程
    /// </summary>
    public class ProcedureCheckVersion : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            MainEntry.CheckVersion.VersionFile.SetAssetVersion(null);//不检测版本号, 而是直接检测MD5
            MainEntry.CheckVersion.CheckVersionComplete = () => GameEntry.Procedure.ChangeState(ProcedureState.Preload);
            MainEntry.CheckVersion.CheckVersionChange();

        }
    }
}
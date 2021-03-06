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
            GameEntry.Resource.InitStreamingAssetsBundleInfo();
        }
    }
}
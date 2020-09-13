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
#if ASSETBUNDLE
            GameEntry.Resource.InitStreamingAssetsBundleInfo();
#else
			GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#endif
		}

		internal override void OnUpdate()
        {
            base.OnUpdate();

        }

		internal override void OnLeave()
        {
            base.OnLeave();

        }

    }
}
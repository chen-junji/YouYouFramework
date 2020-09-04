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

        public override void OnEnter()
        {
            base.OnEnter();
#if ASSETBUNDLE
            GameEntry.Resource.InitStreamingAssetsBundleInfo();
#else
			GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#endif
		}

		public override void OnUpdate()
        {
            base.OnUpdate();

        }

        public override void OnLeave()
        {
            base.OnLeave();

        }

    }
}
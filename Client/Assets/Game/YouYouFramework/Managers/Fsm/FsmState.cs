using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 状态机的状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FsmState<T> where T : class
    {
        /// <summary>
        /// 持有该状态的状态机
        /// </summary>
        public Fsm<T> CurrFsm;

        internal virtual void OnInit() { }
        /// <summary>
        /// 进入状态
        /// </summary>
        internal virtual void OnEnter() { }

		/// <summary>
		/// 执行状态
		/// </summary>
		internal virtual void OnUpdate() { }

		/// <summary>
		/// 离开状态
		/// </summary>
		internal virtual void OnLeave() { }

		/// <summary>
		/// 状态机销毁时调用
		/// </summary>
		internal virtual void OnDestroy() { }

    }
}
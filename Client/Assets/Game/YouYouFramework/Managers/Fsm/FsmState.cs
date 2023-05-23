using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 状态机的状态
    /// </summary>
    /// <typeparam name="T">状态机</typeparam>
    public abstract class FsmState<T> where T : class
    {
        /// <summary>
        /// 所属状态机
        /// </summary>
        public Fsm<T> CurrFsm;
        /// <summary>
        /// 所属状态机管理器
        /// </summary>
        protected T FsmMgr;

        /// <summary>
        /// 当前状态的内部行为是否执行完毕
        /// </summary>
        public bool ActionComplete { get; protected set; }

        internal virtual void OnInit()
        {
            FsmMgr = CurrFsm.Owner;
        }
        /// <summary>
        /// 进入状态
        /// </summary>
        internal virtual void OnEnter()
        {
            ActionComplete = false;
        }

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
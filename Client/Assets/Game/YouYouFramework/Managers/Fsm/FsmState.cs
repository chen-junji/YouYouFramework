using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
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
        /// 状态过渡线列表
        /// </summary>
        public List<FsmTransition> Transitions = new();

        /// <summary>
        /// 状态需要用到的信息列表
        /// </summary>
        protected List<object> InfoList = new();

        public void SetInfoList(List<object> infoList)
        {
            InfoList = infoList;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal virtual void OnInit()
        {
            FsmMgr = CurrFsm.Owner;
        }

        /// <summary>
        /// 进入状态
        /// </summary>
        /// <param name="lastState">上一个状态</param>
        public virtual void OnEnter(int lastState) { }

        /// <summary>
        /// 进入当前状态后, 每帧调用
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间, 以秒为单位</param>
        public virtual void OnUpdate(float elapseSeconds) { }

        /// <summary>
        /// 离开
        /// </summary>
        /// <param name="newState">将要切换到的状态</param>
        public virtual void OnLeave(int newState) { }

        /// <summary>
        /// 状态机销毁时调用
        /// </summary>
        internal virtual void OnDestroy() { }

    }
}
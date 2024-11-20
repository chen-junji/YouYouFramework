using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 流程管理器, 本质上就是个状态机, 使用方法可以参考TestFsm脚本
    /// </summary>
    public class ProcedureManager
    {
        /// <summary>
        /// 流程状态
        /// </summary>
        public enum EState
        {
            /// <summary>
            /// 初始化
            /// </summary>
            Launch,
            /// <summary>
            /// 预加载
            /// </summary>
            Preload,
            /// <summary>
            /// 登录
            /// </summary>
            Login,
            /// <summary>
            /// 游戏主流程
            /// </summary>
            Main
        }

        /// <summary>
        /// 当前流程状态机
        /// </summary>
        public Fsm<ProcedureManager> CurrFsm { get; private set; }

        /// <summary>
        /// 当前流程状态Type
        /// </summary>
        public EState CurrProcedureState
        {
            get
            {
                return (EState)CurrFsm.CurrStateType;
            }
        }

        internal void Init()
        {
            //得到枚举的长度
            int count = Enum.GetNames(typeof(EState)).Length;
            FsmState<ProcedureManager>[] states = new FsmState<ProcedureManager>[count];

            states[(byte)EState.Launch] = new ProcedureLaunch();
            states[(byte)EState.Preload] = new ProcedurePreload();
            states[(byte)EState.Login] = new ProcedureLogin();
            states[(byte)EState.Main] = new ProcedureMain();

            //创建流程的状态机
            CurrFsm = GameEntry.Fsm.Create(this, states);
        }
        internal void OnUpdate()
        {
            CurrFsm.OnUpdate();
        }
        public void ChangeState(EState state)
        {
            CurrFsm.ChangeState((sbyte)state);
        }
        public void SetInfoList(EState state, List<object> infoList)
        {
            var fsmState = CurrFsm.GetState((sbyte)state);
            fsmState?.SetInfoList(infoList);
        }

    }
}
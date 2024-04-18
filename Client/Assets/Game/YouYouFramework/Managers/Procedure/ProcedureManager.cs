using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public enum ProcedureState
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
    /// 流程管理器, 本质上就是个状态机, 使用方法可以参考TestFsm脚本
    /// </summary>
    public class ProcedureManager 
    {
        /// <summary>
        /// 当前流程状态机
        /// </summary>
        public Fsm<ProcedureManager> CurrFsm { get; private set; }

        /// <summary>
        /// 当前流程状态Type
        /// </summary>
        public ProcedureState CurrProcedureState
        {
            get
            {
                return (ProcedureState)CurrFsm.CurrStateType;
            }
        }

        internal void Init()
        {
            //得到枚举的长度
            int count = Enum.GetNames(typeof(ProcedureState)).Length;
            FsmState<ProcedureManager>[] states = new FsmState<ProcedureManager>[count];
            states[(byte)ProcedureState.Launch] = new ProcedureLaunch();
            states[(byte)ProcedureState.Preload] = new ProcedurePreload();
            states[(byte)ProcedureState.Login] = new ProcedureLogin();
            states[(byte)ProcedureState.Main] = new ProcedureMain();

            //创建流程的状态机
            CurrFsm = GameEntry.Fsm.Create(this, states);
        }
        internal void OnUpdate()
        {
            CurrFsm.OnUpdate();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState(ProcedureState state)
        {
            CurrFsm.ChangeState((sbyte)state);
        }

        public void SetData<TData>(string key, TData value)
        {
            CurrFsm.SetData(key, value);
        }
        public TData GetDada<TData>(string key)
        {
            return CurrFsm.GetDada<TData>(key);
        }
    }
}
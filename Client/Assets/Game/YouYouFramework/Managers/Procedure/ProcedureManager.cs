using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public enum ProcedureState
    {
        None,
        /// <summary>
        /// 初始化
        /// </summary>
        Launch,
        /// <summary>
        /// 检查更新
        /// </summary>
        CheckVersion,
        /// <summary>
        /// 预加载
        /// </summary>
        Preload,
        /// <summary>
        /// 登录
        /// </summary>
        Login,
        /// <summary>
        /// 游戏
        /// </summary>
        Game
    }
    /// <summary>
    /// 流程管理器
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
            states[(byte)ProcedureState.None] = new ProcedureNone();
            states[(byte)ProcedureState.Launch] = new ProcedureLaunch();
            states[(byte)ProcedureState.CheckVersion] = new ProcedureCheckVersion();
            states[(byte)ProcedureState.Preload] = new ProcedurePreload();
            states[(byte)ProcedureState.Login] = new ProcedureLogin();
            states[(byte)ProcedureState.Game] = new ProcedureGame();

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetData<TData>(string key, TData value)
        {
            CurrFsm.SetData(key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TData GetDada<TData>(string key)
        {
            return CurrFsm.GetDada<TData>(key);
        }
    }
}
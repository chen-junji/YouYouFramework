using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYouFramework
{
	/// <summary>
	/// 状态机
	/// </summary>
	/// <typeparam name="T">FSMManager</typeparam>
	public class Fsm<T> : FsmBase where T : class
	{
		/// <summary>
		/// 状态机拥有者
		/// </summary>
		public T Owner { get; private set; }

		/// <summary>
		/// 当前状态
		/// </summary>
		public FsmState<T> CurrState { get; private set; }

        /// <summary>
        /// 状态字典
        /// </summary>
        private Dictionary<sbyte, FsmState<T>> m_StateDic;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fsmId">状态机编号</param>
        /// <param name="owner">拥有者</param>
        /// <param name="states">状态数组</param>

        public Fsm(int fsmId, T owner, FsmState<T>[] states) : base(fsmId)
		{
			m_StateDic = new Dictionary<sbyte, FsmState<T>>();
			Owner = owner;

			//把状态加入字典
			int len = states.Length;
			for (int i = 0; i < len; i++)
			{
				FsmState<T> state = states[i];
                if (state != null)
                {
                    state.OnInit(this, i);
                }
                m_StateDic[(sbyte)i] = state;
            }

            //设置默认状态
            CurrStateType = -1;
        }

        private float elapseSeconds;
        internal void OnUpdate()
        {
            if (CurrState != null)
            {
                elapseSeconds += Time.deltaTime;
                CurrState.OnUpdate(elapseSeconds);
            }
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <param name="stateType">状态Type</param>
        /// <returns>状态</returns>
        public FsmState<T> GetState(sbyte stateType)
		{
			FsmState<T> state = null;
			m_StateDic.TryGetValue(stateType, out state);
			return state;
		}

        /// <summary>
        /// 根据Transitions来进行状态切换
        /// </summary>
        /// <param name="newState">目标状态</param>
        /// <param name="info">自定义参数</param>
        /// <returns>最新状态</returns>
        public FsmState<T> ChangeState(sbyte newState)
        {
            if (m_StateDic == null || !m_StateDic.ContainsKey(newState) || m_StateDic[newState] == null)
            {
                Debug.LogError($"BaseStateMachine.ChangeState error! state={newState}");
                return CurrState;
            }

            if (CurrStateType == newState)
            {
                return CurrState;
            }

            if (CurrState != null)
            {
                CurrState.OnLeave(newState);
            }

            //进入新状态
            int lastState = CurrStateType;
            CurrStateType = newState;
            CurrState = m_StateDic[newState];
            elapseSeconds = 0;
            CurrState.OnEnter(lastState);
            return CurrState;
        }

        /// <summary>
        /// 关闭状态机
        /// </summary>
        public override void Destroy()
		{
			if (CurrState != null)
			{
				CurrState.OnLeave(0);
            }
            CurrState = null;

            foreach (KeyValuePair<sbyte, FsmState<T>> state in m_StateDic)
			{
				if (state.Value == null) continue;
				state.Value.OnDestroy();
			}
			m_StateDic.Clear();
		}

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
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
		private FsmState<T> m_CurrState;

		/// <summary>
		/// 状态字典
		/// </summary>
		private Dictionary<sbyte, FsmState<T>> m_StateDic;

		/// <summary>
		/// 参数字典
		/// </summary>
		private Dictionary<string, VariableBase> m_ParamDic;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="fsmId">状态机编号</param>
		/// <param name="owner">拥有者</param>
		/// <param name="states">状态数组</param>

		public Fsm(int fsmId, T owner, FsmState<T>[] states) : base(fsmId)
		{
			m_StateDic = new Dictionary<sbyte, FsmState<T>>();
			m_ParamDic = new Dictionary<string, VariableBase>();
			Owner = owner;

			//把状态加入字典
			int len = states.Length;
			for (int i = 0; i < len; i++)
			{
				FsmState<T> state = states[i];
				if (state != null)
				{
					state.CurrFsm = this;
					state.OnInit();
				}
				m_StateDic[(sbyte)i] = state;
			}

			//设置默认状态
			CurrStateType = -1;
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

		internal void OnUpdate()
		{
			if (m_CurrState != null)
			{
				m_CurrState.OnUpdate();
			}
		}

		/// <summary>
		/// 切换状态
		/// </summary>
		/// <param name="newState"></param>
		public FsmState<T> ChangeState(sbyte newState)
		{
			if (CurrStateType == newState) return m_CurrState;

			if (m_CurrState != null)
			{
				m_CurrState.OnLeave();
			}
			CurrStateType = newState;
			m_CurrState = m_StateDic[CurrStateType];

			//进入新状态
			m_CurrState.OnEnter();
			return m_CurrState;
		}

		/// <summary>
		/// 设置参数值
		/// </summary>
		/// <typeparam name="TData">参数类型</typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetData<TData>(string key, TData value)
		{
			VariableBase itemBase = null;
			if (m_ParamDic.TryGetValue(key, out itemBase))
			{
				Variable<TData> item = itemBase as Variable<TData>;
				item.Value = value;
				m_ParamDic[key] = item;
			}
			else
			{
				//参数不存在
				Variable<TData> item = new Variable<TData>();
				item.Value = value;
				m_ParamDic[key] = item;
			}
		}
		/// <summary>
		/// 获取参数值
		/// </summary>
		/// <typeparam name="TData">参数类型</typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public TData GetDada<TData>(string key)
		{
			VariableBase itemBase = null;
			if (m_ParamDic.TryGetValue(key, out itemBase))
			{
				Variable<TData> item = itemBase as Variable<TData>;
				return item.Value;
			}
			return default(TData);
		}

		/// <summary>
		/// 关闭状态机
		/// </summary>
		public override void ShutDown()
		{
			if (m_CurrState != null)
			{
				m_CurrState.OnLeave();
			}

			foreach (KeyValuePair<sbyte, FsmState<T>> state in m_StateDic)
			{
				if (state.Value == null) continue;
				state.Value.OnDestroy();
			}
			m_StateDic.Clear();
			m_ParamDic.Clear();
		}
	}
}
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
		/// 参数字典
		/// </summary>
		private Dictionary<string, VariableBase> m_ParamDic;

        /// <summary>
        /// 任意状态, 过渡线列表
        /// </summary>
        public List<FsmTransition> AnyStateTransitions = new();

        public Action ActionStateChange;

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
        }

        private float elapseSeconds;
        internal void OnUpdate()
        {
            //设置默认状态
            if (CurrState == null)
            {
                ChangeState(0);
            }
            if (CurrState != null)
            {
                elapseSeconds += Time.deltaTime;
                CurrState.OnUpdate(elapseSeconds);
            }

            CheckChangeState();
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
        /// <returns>切换是否成功</returns>
        private bool ChangeState(sbyte newState)
        {
            if (m_StateDic == null || !m_StateDic.ContainsKey(newState) || m_StateDic[newState] == null)
            {
                Debug.LogError($"BaseStateMachine.ChangeState error! state={newState}");
                return false;
            }

            if (CurrState != null)
			{
				CurrState.OnLeave(newState);
			}
			CurrStateType = newState;
			CurrState = m_StateDic[CurrStateType];

            //进入新状态
            int lastState = CurrStateType;
            CurrStateType = newState;
            CurrState = m_StateDic[newState];
            elapseSeconds = 0;
            CurrState.OnEnter(lastState);
            ActionStateChange?.Invoke();
            return true;
        }

		/// <summary>
		/// 设置参数值
		/// </summary>
		/// <typeparam name="TData">参数类型</typeparam>
		public void SetParam<TData>(string key, TData value)
		{
			VariableBase itemBase = null;
			if (m_ParamDic.TryGetValue(key, out itemBase))
			{
				Variable<TData> item = itemBase as Variable<TData>;
				item.Value = value;
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
		public TData GetParam<TData>(string key)
		{
			VariableBase itemBase = null;
			if (m_ParamDic.TryGetValue(key, out itemBase))
			{
				Variable<TData> item = itemBase as Variable<TData>;
				return item.Value;
			}
			return default(TData);
        }
        private void ResetParam()
        {
            m_ParamDic.Clear();
        }

        //条件切换的一种 触发Trigger 每次切换条件时 清空Trigger
        private Dictionary<string, bool> triggerDic = new Dictionary<string, bool>();
        public void SetTrigger(string key)
        {
            triggerDic[key] = true;
            //CheckChangeState();
        }
        public bool GetTrigger(string key)
        {
            if (triggerDic.TryGetValue(key, out var isTrigger))
            {
                return isTrigger;
            }
            return false;
        }
        private bool CheckTriggers(List<string> list)
        {
            bool isTrigger = true;
            foreach (string key in list)
            {
                if (triggerDic.TryGetValue(key, out var value))
                {
                    if (value == false)
                    {
                        isTrigger = false;
                    }
                }
                else
                {
                    isTrigger = false;
                }
            }
            return isTrigger;
        }
        private void ResetTrigger()
        {
            triggerDic.Clear();
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

			foreach (KeyValuePair<sbyte, FsmState<T>> state in m_StateDic)
			{
				if (state.Value == null) continue;
				state.Value.OnDestroy();
			}
			m_StateDic.Clear();
			m_ParamDic.Clear();
		}

        //当参数面板有变化时, 检查是否需要切换状态
        private void CheckChangeState()
        {
            foreach (var fsmTransition in AnyStateTransitions)
            {
                if (fsmTransition.CanTransitions() && CheckTriggers(fsmTransition.FsmConditionTriggers))
                {
                    //状态过渡线的过渡条件是空的,  不允许过渡状态
                    if (fsmTransition.FsmConditions.Count == 0 && fsmTransition.FsmConditionTriggers.Count == 0)
                    {
                        continue;
                    }

                    foreach (var item in fsmTransition.FsmConditionTriggers)
                    {
                        triggerDic.Remove(item);
                    }
                    ChangeState(fsmTransition.TargetState);
                    return;
                }
            }

            if (CurrState != null)
            {
                foreach (var fsmTransition in CurrState.Transitions)
                {
                    if (fsmTransition.CanTransitions() && CheckTriggers(fsmTransition.FsmConditionTriggers))
                    {
                        //状态过渡线的过渡条件是空的,  不允许过渡状态
                        if (fsmTransition.FsmConditions.Count == 0 && fsmTransition.FsmConditionTriggers.Count == 0)
                        {
                            continue;
                        }

                        foreach (var item in fsmTransition.FsmConditionTriggers)
                        {
                            triggerDic.Remove(item);
                        }
                        ChangeState(fsmTransition.TargetState);
                        return;
                    }
                }
            }
        }

    }
}
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
	public class GPFsm<T> : GPFsmBase where T : class
	{
		/// <summary>
		/// 状态机拥有者
		/// </summary>
		public T Owner { get; private set; }

		/// <summary>
		/// 当前状态
		/// </summary>
		public GPFsmState<T> CurrState { get; private set; }

        /// <summary>
        /// 状态字典
        /// </summary>
        private Dictionary<sbyte, GPFsmState<T>> m_StateDic;

		/// <summary>
		/// 参数字典
		/// </summary>
		private Dictionary<string, VariableBase> m_ParamDic;

        /// <summary>
        /// 任意状态, 过渡线列表
        /// </summary>
        public List<GPFsmTransition> AnyStateTransitions = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">拥有者</param>
        /// <param name="states">状态数组</param>

        public GPFsm(T owner, GPFsmState<T>[] states)
		{
			m_StateDic = new Dictionary<sbyte, GPFsmState<T>>();
			m_ParamDic = new Dictionary<string, VariableBase>();
			Owner = owner;

			//把状态加入字典
			int len = states.Length;
			for (int i = 0; i < len; i++)
			{
				GPFsmState<T> state = states[i];
                if (state != null)
                {
                    state.OnInit(this, i);
                }
                m_StateDic[(sbyte)i] = state;
            }

            //设置默认状态
            ChangeState(0);
        }

        private float elapseSeconds;
        internal void OnUpdate()
        {
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
        public GPFsmState<T> GetState(sbyte stateType)
		{
			GPFsmState<T> state = null;
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

            //进入新状态
            int lastState = CurrStateType;
            CurrStateType = newState;
            CurrState = m_StateDic[newState];
            elapseSeconds = 0;
            CurrState.OnEnter(lastState);
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

        //条件切换的一种 触发Trigger 每次切换条件时 重置Trigger
        private Dictionary<string, bool> triggerDic = new Dictionary<string, bool>();
        public void SetTrigger(string key)
        {
            triggerDic[key] = true;

            //Debug.Log("SetTrigger==" + key + "==" + uuid);
        }
        private bool CheckTriggers(List<string> list)
        {
            foreach (string key in list)
            {
                if (triggerDic.TryGetValue(key, out var value))
                {
                    if (value == false)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
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

            foreach (KeyValuePair<sbyte, GPFsmState<T>> state in m_StateDic)
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
                //状态过渡线的过渡条件是空的,  不允许过渡状态
                if (fsmTransition.FsmConditions.Count == 0 && fsmTransition.FsmConditionTriggers.Count == 0)
                {
                    continue;
                }

                if (fsmTransition.CanTransitions() && CheckTriggers(fsmTransition.FsmConditionTriggers))
                {
                    foreach (var item in fsmTransition.FsmConditionTriggers)
                    {
                        //if (triggerDic.TryGetValue(item, out var value))
                        //{
                        //    if (value)
                        //    {
                        //        Debug.Log("条件满足了, 所以切状态了==" + item + "==" + uuid);
                        //    }
                        //}
                        triggerDic.Remove(item);
                    }
                    ChangeState(fsmTransition.TargetState);
                    return;
                }
            }

            if (CurrState == null) return;

            foreach (var fsmTransition in CurrState.Transitions)
            {
                //状态过渡线的过渡条件是空的,  不允许过渡状态
                if (fsmTransition.FsmConditions.Count == 0 && fsmTransition.FsmConditionTriggers.Count == 0)
                {
                    continue;
                }

                if (fsmTransition.CanTransitions() && CheckTriggers(fsmTransition.FsmConditionTriggers))
                {
                    foreach (var item in fsmTransition.FsmConditionTriggers)
                    {
                        //if (triggerDic.TryGetValue(item, out var value))
                        //{
                        //    if (value)
                        //    {
                        //        Debug.Log("条件满足了, 所以切状态了==" + item + "==" + uuid);
                        //    }
                        //}
                        triggerDic.Remove(item);
                    }
                    ChangeState(fsmTransition.TargetState);
                    return;
                }
            }
        }
    }
}
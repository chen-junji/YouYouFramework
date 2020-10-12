using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class InputManager : ManagerBase, IDisposable
	{
		private InputCtrlBase m_InputCtrl;
		/// <summary>
		/// 按下并抬起 光标都在同一位置 则触发一次
		/// </summary>
		public event BaseAction<TouchEventData> OnClick;
		/// <summary>
		/// 按下 触发一次
		/// </summary>
		public event BaseAction<TouchEventData> OnBeginDrag;
		/// <summary>
		/// 抬起 触发一次
		/// </summary>
		public event BaseAction<TouchEventData> OnEndDrag;
		/// <summary>
		/// 拖拽滑动 Axis!=(0,0)时,也就是有方向的话 持续调用
		/// </summary>
		public event BaseAction<TouchDirection, TouchEventData> OnDrag;
		/// <summary>
		/// 放大缩小 Axis!=0时, 持续调用
		/// </summary>
		public event BaseAction<ZoomType> OnZoom;

		internal override void Init()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			m_InputCtrl = new StandalonInputCtrl(t => OnClick?.Invoke(t),
				t => OnBeginDrag?.Invoke(t),
				t => OnEndDrag?.Invoke(t),
				(t1, t2) => OnDrag?.Invoke(t1, t2),
				t => OnZoom?.Invoke(t));
#else
			m_InputCtrl = new MobileInputCtrl(t => OnClick?.Invoke(t),
				t => OnBeginDrag?.Invoke(t),
				t => OnEndDrag?.Invoke(t),
				(t1, t2) => OnDrag?.Invoke(t1, t2),
				t => OnZoom?.Invoke(t));
#endif
		}

		internal void OnUpdate()
		{
			m_InputCtrl.OnUpdate();
		}
		public void Dispose()
		{

		}
	}
}
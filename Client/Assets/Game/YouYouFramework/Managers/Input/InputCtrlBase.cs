using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public abstract class InputCtrlBase
	{

		protected bool isBeginDrag;
		protected bool isEndDrag;
		protected bool isDraging;
		private TouchEventData touchEventData;
		protected TouchEventData TouchEventData
		{
			get
			{
				if (touchEventData == null) touchEventData = new TouchEventData();
				return touchEventData;
			}
		}

		protected Action<TouchEventData> OnClick { get; private set; }
		protected Action<TouchEventData> OnBeginDrag { get; private set; }
		protected Action<TouchEventData> OnEndDrag { get; private set; }
		protected Action<TouchDirection, TouchEventData> OnDrag { get; private set; }
		protected Action<ZoomType> OnZoom { get; private set; }

		public InputCtrlBase(Action<TouchEventData> onClick,
			Action<TouchEventData> onBeginDrag,
			Action<TouchEventData> onEndDrag,
			Action<TouchDirection, TouchEventData> onDrag,
			Action<ZoomType> onZoom)
		{
			OnClick = onClick;
			OnBeginDrag = onBeginDrag;
			OnEndDrag = onEndDrag;
			OnDrag = onDrag;
			OnZoom = onZoom;
		}

		internal abstract void OnUpdate();

		//点击相关
		protected abstract bool Click();

		//拖拽相关
		protected abstract bool BeginDrag();
		protected abstract bool EndDrag();
		protected abstract bool Drag();

		//滑动相关
		protected abstract bool Move(TouchDirection touchDirection, TouchEventData touchEventData);

		//缩放相关
		protected abstract bool Zoom();
	}
}
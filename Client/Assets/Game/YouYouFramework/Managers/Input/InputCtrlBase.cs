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

		protected BaseAction<TouchEventData> OnClick { get; private set; }
		protected BaseAction<TouchEventData> OnBeginDrag { get; private set; }
		protected BaseAction<TouchEventData> OnEndDrag { get; private set; }
		protected BaseAction<TouchDirection, TouchEventData> OnDrag { get; private set; }
		protected BaseAction<ZoomType> OnZoom { get; private set; }

		public InputCtrlBase(BaseAction<TouchEventData> onClick,
			BaseAction<TouchEventData> onBeginDrag,
			BaseAction<TouchEventData> onEndDrag,
			BaseAction<TouchDirection, TouchEventData> onDrag,
			BaseAction<ZoomType> onZoom)
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
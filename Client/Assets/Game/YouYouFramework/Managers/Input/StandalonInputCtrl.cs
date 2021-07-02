using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYou
{
	public class StandalonInputCtrl : InputCtrlBase
	{
		public StandalonInputCtrl(Action<TouchEventData> onClick, Action<TouchEventData> onBeginDrag, Action<TouchEventData> onEndDrag, Action<TouchDirection, TouchEventData> onDrag, Action<ZoomType> onZoom) : base(onClick, onBeginDrag, onEndDrag, onDrag, onZoom)
		{
		}

		internal override void OnUpdate()
		{
			if (!isBeginDrag)
			{
				BeginDrag();
			}
			if (isBeginDrag)
			{
				Drag();
			}

			if (!isBeginDrag && TouchEventData.totalDelta.magnitude == 0 && Click())
			{
				OnClick?.Invoke(TouchEventData);
			}

			Zoom();
		}

		protected override bool Click()
		{
			bool isClick = false;
			if (Input.GetMouseButtonDown(0))
			{
				TouchEventData.pressPosition = Input.mousePosition;
				TouchEventData.lastPosition = Input.mousePosition;
				TouchEventData.touchTime = Time.time;
			}
			if (Input.GetMouseButtonUp(0))
			{
				TouchEventData.touchTime = Time.time - TouchEventData.touchTime;
				isClick = true;
			}
			return isClick;
		}

		protected override bool BeginDrag()
		{
			if (Input.GetMouseButtonDown(0))
			{
				TouchEventData.pressPosition = Input.mousePosition;
				TouchEventData.startPosition = Input.mousePosition;
				TouchEventData.lastPosition = Input.mousePosition;
				TouchEventData.touchTime = Time.time;

				isBeginDrag = true;
				isEndDrag = false;

				OnBeginDrag?.Invoke(TouchEventData);
				return true;
			}
			return false;
		}

		protected override bool Drag()
		{
			if (isBeginDrag)
			{
				if (Input.GetMouseButton(0))
				{
					TouchEventData.delta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - TouchEventData.lastPosition;
					TouchEventData.totalDelta = TouchEventData.lastPosition - new Vector2(TouchEventData.startPosition.x, TouchEventData.startPosition.y);
					TouchEventData.mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

					if (TouchEventData.delta.magnitude > 0f)
					{
						isDraging = true;
						if (TouchEventData.delta.y > TouchEventData.delta.x && TouchEventData.delta.y > -TouchEventData.delta.x)
						{
							Move(TouchDirection.MoveUp, TouchEventData);
						}
						else if (TouchEventData.delta.y < TouchEventData.delta.x && TouchEventData.delta.y < -TouchEventData.delta.x)
						{
							Move(TouchDirection.MoveDown, TouchEventData);
						}
						else if (TouchEventData.delta.y < TouchEventData.delta.x && TouchEventData.delta.y > -TouchEventData.delta.x)
						{
							Move(TouchDirection.MoveRight, TouchEventData);
						}
						else if (TouchEventData.delta.y > TouchEventData.delta.x && TouchEventData.delta.y < -TouchEventData.delta.x)
						{
							Move(TouchDirection.MoveLeft, TouchEventData);
						}
						else
						{
							Move(TouchDirection.MoveNone, TouchEventData);
						}
					}
					TouchEventData.lastPosition = Input.mousePosition;
					return true;
				}

				if (Input.GetMouseButtonUp(0))
				{
					EndDrag();
				}
			}
			return false;
		}

		protected override bool EndDrag()
		{
			if (isBeginDrag)
			{
				TouchEventData.lastPosition = Input.mousePosition;
				TouchEventData.touchTime = Time.time - TouchEventData.touchTime;
				isBeginDrag = false;
				isDraging = false;
				isEndDrag = true;

				OnEndDrag?.Invoke(TouchEventData);
				return true;
			}
			return false;
		}

		protected override bool Zoom()
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0)
			{
				OnZoom?.Invoke(ZoomType.Out);
				return true;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") > 0)
			{
				OnZoom?.Invoke(ZoomType.In);
				return true;
			}
			return false;
		}

		protected override bool Move(TouchDirection touchDirection, TouchEventData touchEventData)
		{
			if (touchDirection != TouchDirection.MoveNone)
			{
				OnDrag?.Invoke(touchDirection, touchEventData);
				return true;
			}
			return false;
		}
	}
}
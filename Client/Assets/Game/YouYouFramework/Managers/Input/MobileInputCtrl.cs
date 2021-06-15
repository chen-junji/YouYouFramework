using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class MobileInputCtrl : InputCtrlBase
	{
		private sbyte prevFinger = -1;
		private byte prevTouchCount = 0;
		private int currTouchFingerId = 1;
		private Touch currTouch = new Touch();

		private Vector2 tempFinger1Pos;
		private Vector2 oldFinger1Pos;

		private Vector2 tempFinger2Pos;
		private Vector2 oldFinger2Pos;

		public MobileInputCtrl(Action<TouchEventData> onClick, Action<TouchEventData> onBeginDrag, Action<TouchEventData> onEndDrag, Action<TouchDirection, TouchEventData> onDrag, Action<ZoomType> onZoom) : base(onClick, onBeginDrag, onEndDrag, onDrag, onZoom)
		{
		}

		protected override bool BeginDrag()
		{
			if (Input.touchCount > 0)
			{
				currTouch = Input.GetTouch(0);
				currTouchFingerId = Input.GetTouch(0).fingerId;
				TouchEventData.pressPosition = Input.GetTouch(0).position;
				TouchEventData.startPosition = Input.GetTouch(0).position;
				TouchEventData.lastPosition = Input.GetTouch(0).position;
				TouchEventData.touchTime = Time.time;

				prevFinger = 1;
				isBeginDrag = true;
				isEndDrag = false;

				OnBeginDrag?.Invoke(TouchEventData);
				return true;
			}
			return false;
		}

		protected override bool Click()
		{
			return false;
		}

		protected override bool Drag()
		{
			if (Input.touchCount == 0)
			{
				prevTouchCount = 0;
				currTouchFingerId = -1;

				if (isBeginDrag) EndDrag();
				return false;
			}
			if (Input.touchCount == 1)
			{
				if (prevTouchCount == 2)
				{
					TouchEventData.lastPosition = Input.GetTouch(0).position;
					currTouchFingerId = Input.GetTouch(0).fingerId;
					prevTouchCount = 1;
				}
			}
			else if (Input.touchCount == 2)
			{
				if (prevTouchCount < 2) prevTouchCount = 2;
			}

			for (int i = 0; i < Input.touchCount; i++)
			{
				if (Input.GetTouch(i).fingerId == currTouchFingerId)
				{
					currTouch = Input.GetTouch(i);
					break;
				}
			}

			if (currTouch.phase != TouchPhase.Ended && currTouch.phase != TouchPhase.Canceled)
			{
				Vector2 touPos = currTouch.position;
				TouchEventData.delta = touPos - TouchEventData.lastPosition;
				TouchEventData.totalDelta = TouchEventData.lastPosition - new Vector2(TouchEventData.startPosition.x, TouchEventData.startPosition.y);
				TouchEventData.mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
				//TouchEventData.deltaPosition = currTouch.deltaPosition;

				if (TouchEventData.delta.magnitude > 0f)
				{
					isDraging = true;
					prevFinger = 2;
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
			return false;
		}

		protected override bool EndDrag()
		{
			if (isBeginDrag)
			{
				TouchEventData.lastPosition = currTouch.position;
				TouchEventData.touchTime = Time.time - TouchEventData.touchTime;
				isBeginDrag = false;
				isDraging = false;
				isEndDrag = true;
				OnEndDrag?.Invoke(TouchEventData);

				if (prevFinger == 1) prevFinger = 3;
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

		protected override bool Zoom()
		{
			if (Input.touchCount > 1)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
				{
					tempFinger1Pos = Input.GetTouch(0).position;
					tempFinger2Pos = Input.GetTouch(1).position;

					if (Vector2.Distance(oldFinger1Pos, oldFinger2Pos) < Vector2.Distance(tempFinger1Pos, tempFinger2Pos))
					{
						OnZoom?.Invoke(ZoomType.In);
					}
					else
					{
						OnZoom?.Invoke(ZoomType.Out);
					}
					oldFinger1Pos = tempFinger1Pos;
					oldFinger2Pos = tempFinger2Pos;
					return true;
				}
			}
			return false;
		}

		internal override void OnUpdate()
		{
			if (Input.touchCount == 0)
			{
				prevTouchCount = 0;
				currTouchFingerId = -1;
				if (!isBeginDrag && prevFinger == 3)
				{
					prevFinger = -1;
					OnClick?.Invoke(TouchEventData);
				}
				if (isBeginDrag) EndDrag();
				return;
			}

			if (!isBeginDrag) BeginDrag();
			else Drag();

			Zoom();
		}
	}
}

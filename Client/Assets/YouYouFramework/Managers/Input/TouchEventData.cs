using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class TouchEventData
	{
		public Vector2 pressPosition;
		public Vector2 delta;
		public Vector2 totalDelta;
		public Vector2 startPosition;
		public Vector2 lastPosition;
		public float touchTime;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class TouchEventData
	{
		//°´ÏÂÎ»ÖÃ
		public Vector2 pressPosition;
		//Ïà¶ÔÓÚÉÏÒ»Ö¡Î»ÖÃµÄÆ«ÒÆÁ¿
		public Vector2 delta;
		//Ïà¶ÔÓÚ¿ªÊ¼Î»ÖÃµÄÆ«ÒÆÁ¿
		public Vector2 totalDelta;
		//¿ªÊ¼Î»ÖÃ
		public Vector2 startPosition;
		//ÉÏÒ»Ö¡Î»ÖÃ
		public Vector2 lastPosition;
		public Vector2 mouseAxis;
		public float touchTime;
	}
}
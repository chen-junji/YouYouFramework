using YouYou;
using UnityEngine;
using System;

public class CircleForm : UIFormBase
{
	[SerializeField] private Transform circleTrans;
	private void Update()
	{
		circleTrans.Rotate(0, 0, -5, Space.Self);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAndroidSDK
{
	void DoAction(string actionName, string param, Action<EventArgs> callBack);
}

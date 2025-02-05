using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// Input检测, 空状态
/// </summary>
public class StateNone : VirtualInput
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override float GetAxis(string name, bool raw)
    {
        return 0;
    }
    public override void SetAxis(string name, float value)
    {
    }

}
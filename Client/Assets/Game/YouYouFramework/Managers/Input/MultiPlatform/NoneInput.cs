using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// Input检测, 空状态
/// </summary>
public class StateNone : VirtualInput
{
    internal override void OnEnter()
    {
        base.OnEnter();
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
    public override void SetAxisNegative(string name)
    {
    }
    public override void SetAxisPositive(string name)
    {
    }
    public override void SetAxisZero(string name)
    {
    }
}
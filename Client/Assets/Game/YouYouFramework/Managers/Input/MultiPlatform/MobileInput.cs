using System;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// Input检测, 移动端状态
/// </summary>
public class MobileInput : VirtualInput
{
    public override void OnEnter(int lastState)
    {
        base.OnEnter(lastState);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheckAddAxes(string name)
    {
        if (!m_VirtualAxes.ContainsKey(name))
        {
            //我们还没有注册这个按钮，所以添加它，发生在构造函数中
            GameEntry.Input.RegisterVirtualAxis(new VirtualAxis(name));
        }
    }

    public override float GetAxis(string name, bool raw)
    {
        CheckAddAxes(name);
        return m_VirtualAxes[name].GetValue;
    }
    public override void SetAxis(string name, float value)
    {
        CheckAddAxes(name);
        m_VirtualAxes[name].Update(value);
    }

}
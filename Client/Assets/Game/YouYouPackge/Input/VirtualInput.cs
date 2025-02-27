using System;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// 所有平台的Input检测父类
/// </summary>
public abstract class VirtualInput : FsmState<InputManager>
{
    protected Dictionary<string, VirtualAxis> m_VirtualAxes = new Dictionary<string, VirtualAxis>();
    protected Dictionary<InputKeyCode, VirtualButton> m_VirtualButtons = new Dictionary<InputKeyCode, VirtualButton>();
    protected List<string> m_AlwaysUseVirtual = new List<string>();


    public override void OnLeave(int newState)
    {
        base.OnLeave(newState);
        foreach (var item in m_VirtualButtons)
        {
            if (item.Value.GetButton || item.Value.GetButtonDown)
            {
                SetButtonUp(item.Key);
            }
        }
    }

    public bool GetButton(InputKeyCode name)
    {
        if (m_VirtualButtons.ContainsKey(name))
        {
            return m_VirtualButtons[name].GetButton;
        }

        InputManager.Instance.RegisterVirtualButton(new VirtualButton(name));
        return m_VirtualButtons[name].GetButton;
    }
    public bool GetButtonDown(InputKeyCode name)
    {
        if (m_VirtualButtons.ContainsKey(name))
        {
            return m_VirtualButtons[name].GetButtonDown;
        }

        InputManager.Instance.RegisterVirtualButton(new VirtualButton(name));
        return m_VirtualButtons[name].GetButtonDown;
    }
    public bool GetButtonUp(InputKeyCode name)
    {
        if (m_VirtualButtons.ContainsKey(name))
        {
            return m_VirtualButtons[name].GetButtonUp;
        }

        InputManager.Instance.RegisterVirtualButton(new VirtualButton(name));
        return m_VirtualButtons[name].GetButtonUp;
    }

    public void SetButtonDown(InputKeyCode name)
    {
        if (!m_VirtualButtons.ContainsKey(name))
        {
            InputManager.Instance.RegisterVirtualButton(new VirtualButton(name));
        }
        m_VirtualButtons[name].Pressed();
    }
    public void SetButtonUp(InputKeyCode name)
    {
        if (!m_VirtualButtons.ContainsKey(name))
        {
            InputManager.Instance.RegisterVirtualButton(new VirtualButton(name));
        }
        m_VirtualButtons[name].Released();
    }

    public void RegisterVirtualAxis(VirtualAxis axis)
    {
        if (m_VirtualAxes.ContainsKey(axis.Name))
        {
            GameEntry.LogError(LogCategory.Framework, "已经有了一个虚拟轴 " + axis.Name + " 重复注册.");
        }
        else
        {
            m_VirtualAxes.Add(axis.Name, axis);

            if (!axis.MatchWithInputManager)
            {
                m_AlwaysUseVirtual.Add(axis.Name);
            }
        }
    }


    public void RegisterVirtualButton(VirtualButton button)
    {
        if (m_VirtualButtons.ContainsKey(button.Name))
        {
            GameEntry.LogError(LogCategory.Framework, "There is already a virtual button named " + button.Name + " registered.");
        }
        else
        {
            m_VirtualButtons.Add(button.Name, button);

            if (!button.MatchWithInputManager)
            {
                m_AlwaysUseVirtual.Add(button.Name.ToString());
            }
        }
    }

    public VirtualAxis VirtualAxisReference(string name)
    {
        if (!m_VirtualAxes.TryGetValue(name, out VirtualAxis axis))
        {
            axis = new VirtualAxis(name);
            InputManager.Instance.RegisterVirtualAxis(axis);
        }
        return axis;
    }
    public VirtualButton VirtualButtonReference(InputKeyCode name)
    {
        if (!m_VirtualButtons.TryGetValue(name, out VirtualButton button))
        {
            button = new VirtualButton(name);
            InputManager.Instance.RegisterVirtualButton(button);
        }
        return button;
    }


    public abstract float GetAxis(string name, bool raw);
    public abstract void SetAxis(string name, float value);

}
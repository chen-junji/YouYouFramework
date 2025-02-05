using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//一个控制器游戏对象(例如。一个虚拟GUI按钮)应该调用这个类的'pressed'函数。然后其他对象可以读取
//该按钮的Get/Down/Up状态
public class VirtualButton
{
    public InputKeyCode Name { get; private set; }
    public bool MatchWithInputManager { get; private set; }

    private int m_LastPressedFrame = -5;
    private int m_ReleasedFrame = -5;
    private bool m_Pressed;

    public VirtualButton(InputKeyCode name) : this(name, true)
    {
    }
    public VirtualButton(InputKeyCode name, bool matchToInputSettings)
    {
        this.Name = name;
        MatchWithInputManager = matchToInputSettings;
    }

    /// <summary>
    /// 按下
    /// </summary>
    public void Pressed()
    {
        if (m_Pressed)
        {
            return;
        }
        m_Pressed = true;
        m_LastPressedFrame = Time.frameCount;
    }

    /// <summary>
    /// 松开
    /// </summary>
    public void Released()
    {
        m_Pressed = false;
        m_ReleasedFrame = Time.frameCount;
    }

    // 这些是按钮的状态，可以通过跨平台输入系统读取
    public bool GetButton
    {
        get { return m_Pressed; }
    }

    public bool GetButtonDown
    {
        get
        {
            return m_LastPressedFrame - Time.frameCount == -1;
        }
    }

    public bool GetButtonUp
    {
        get
        {
            return (m_ReleasedFrame == Time.frameCount - 1);
        }
    }
}
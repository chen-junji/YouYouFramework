using YouYouMain;
using System;
using UnityEngine;
using UnityEngine.Events;
using YouYouFramework;


public class InputManager : SingletonMonoInstansce<InputManager>
{
    public enum EState
    {
        None,
        Touch,
        Keyboard,
    }
    public Fsm<InputManager> CurrFsm { get; private set; }

    private VirtualInput CurrInput;

    public Action<InputKeyCode> ActionInput;


    private void Awake()
    {
        //得到枚举的长度
        int count = Enum.GetNames(typeof(EState)).Length;
        FsmState<InputManager>[] states = new FsmState<InputManager>[count];
        states[(byte)EState.None] = new StateNone();
        states[(byte)EState.Keyboard] = new StandaloneInput();
        states[(byte)EState.Touch] = new MobileInput();

        CurrFsm = GameEntry.Fsm.Create(this, states);
        ChangeState(EState.None);
    }
    private void Update()
    {
        CurrFsm.OnUpdate();
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    public void ChangeState(EState state)
    {
        CurrInput = CurrFsm.ChangeState((sbyte)state) as VirtualInput;
    }

    public void SetEnable(bool enable)
    {
        if (enable)
        {
            EState state;
#if UNITY_STANDALONE
                state = EState.Keyboard;
#elif UNITY_IOS || UNITY_ANDROID
            state = EState.Touch;
#endif
#if UNITY_EDITOR
            state = GameEntry.ParamsSettings.MobileDebug ? EState.Touch : EState.Keyboard;
#endif
            ChangeState(state);
        }
        else
        {
            ChangeState(EState.None);
        }
    }

    public VirtualButton VirtualButtonReference(InputKeyCode name)
    {
        return CurrInput.VirtualButtonReference(name);
    }


    public void RegisterVirtualButton(VirtualButton button)
    {
        CurrInput.RegisterVirtualButton(button);
    }

    public bool GetButton(InputKeyCode name)
    {
        return CurrInput.GetButton(name);
    }
    public bool GetButtonDown(InputKeyCode name)
    {
        return CurrInput.GetButtonDown(name);
    }
    public bool GetButtonUp(InputKeyCode name)
    {
        return CurrInput.GetButtonUp(name);
    }

    public void SetButtonDown(InputKeyCode name)
    {
        CurrInput.SetButtonDown(name);
    }
    public void SetButtonUp(InputKeyCode name)
    {
        CurrInput.SetButtonUp(name);
    }


    public void RegisterVirtualAxis(VirtualAxis axis)
    {
        CurrInput.RegisterVirtualAxis(axis);
    }

    public float GetAxis(string name)
    {
        return GetAxis(name, false);
    }
    public float GetAxisRaw(string name)
    {
        return GetAxis(name, true);
    }
    private float GetAxis(string name, bool raw)
    {
        return CurrInput.GetAxis(name, raw);
    }

    public void SetAxis(string name, float value)
    {
        CurrInput.SetAxis(name, value);
    }

}
using YouYouMain;
using System;
using UnityEngine;
using UnityEngine.Events;


namespace YouYouFramework
{
    public class InputManager
    {
        public enum State
        {
            None,
            Touch,
            KeyboardMouse,
        }
        public Fsm<InputManager> CurrFsm { get; private set; }

        private VirtualInput CurrInput;

        public Action<InputKeyCode> ActionInput;


        public InputManager()
        {
            //得到枚举的长度
            int count = Enum.GetNames(typeof(State)).Length;
            FsmState<InputManager>[] states = new FsmState<InputManager>[count];
            states[(byte)State.None] = new StateNone();
            states[(byte)State.KeyboardMouse] = new StandaloneInput();
            states[(byte)State.Touch] = new MobileInput();

            CurrFsm = GameEntry.Fsm.Create(this, states);
            SetEnable(false);
        }
        internal void OnUpdate()
        {
            CurrFsm.OnUpdate();
        }
        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState(State state)
        {
            CurrInput = CurrFsm.ChangeState((sbyte)state) as VirtualInput;
        }

        public void SetEnable(bool enable)
        {
            if (enable)
            {
                State state;
#if UNITY_STANDALONE
                state = State.KeyboardMouse;
#elif UNITY_IOS || UNITY_ANDROID
                state = State.Touch;
#endif
#if UNITY_EDITOR
                state = MainEntry.ParamsSettings.MobileDebug ? State.Touch : State.KeyboardMouse;
#endif
                ChangeState(state);
            }
            else
            {
                ChangeState(State.None);
            }
        }

        public void RegisterVirtualAxis(VirtualAxis axis)
        {
            CurrInput.RegisterVirtualAxis(axis);
        }

        public void RegisterVirtualButton(VirtualButton button)
        {
            CurrInput.RegisterVirtualButton(button);
        }

        public VirtualButton VirtualButtonReference(InputKeyCode name)
        {
            return CurrInput.VirtualButtonReference(name);
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

        public void SetAxisPositive(string name)
        {
            CurrInput.SetAxisPositive(name);
        }
        public void SetAxisNegative(string name)
        {
            CurrInput.SetAxisNegative(name);
        }
        public void SetAxisZero(string name)
        {
            CurrInput.SetAxisZero(name);
        }
        public void SetAxis(string name, float value)
        {
            CurrInput.SetAxis(name, value);
        }
    }
}

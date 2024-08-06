using YouYouMain;
using System;
using UnityEngine;
using UnityEngine.Events;


namespace YouYouFramework
{
    public class InputManager
    {
        public enum EState
        {
            None,
            Touch,
            Keyboard,
        }
        public class ParamConst
        {
            public const string IntIsEnable = "IntIsEnable";
            public const string TriggerTouch = "TriggerTouch";
            public const string TriggerKeyboard = "TriggerKeyboard";
        }

        public Fsm<InputManager> CurrFsm { get; private set; }

        private VirtualInput CurrInput;

        public Action<InputKeyCode> ActionInput;


        public void Init()
        {
            //得到枚举的长度
            int count = Enum.GetNames(typeof(EState)).Length;
            FsmState<InputManager>[] states = new FsmState<InputManager>[count];
            states[(byte)EState.None] = new StateNone();
            states[(byte)EState.Keyboard] = new StandaloneInput();
            states[(byte)EState.Touch] = new MobileInput();

            CurrFsm = GameEntry.Fsm.Create(this, states);
            CurrFsm.AnyStateTransitions = new()
            {
                new()
                {
                    TargetState = (int)EState.None,
                    FsmConditions = new()
                    {
                        new(()=> !CurrFsm.GetParam<bool>(ParamConst.IntIsEnable)),
                    },
                    FsmConditionTriggers = new()
                    {
                    }
                },
                new()
                {
                    TargetState = (int)EState.Keyboard,
                    FsmConditions = new()
                    {
                        new(()=> CurrFsm.GetParam<bool>(ParamConst.IntIsEnable)),
                    },
                    FsmConditionTriggers = new()
                    {
                        ParamConst.TriggerKeyboard,
                    }
                },
                new()
                {
                    TargetState = (int)EState.Touch,
                    FsmConditions = new()
                    {
                        new(()=> !CurrFsm.GetParam<bool>(ParamConst.IntIsEnable)),
                    },
                    FsmConditionTriggers = new()
                    {
                        ParamConst.TriggerTouch,
                    }
                },
            };
            CurrFsm.ActionStateChange = () =>
            {
                CurrInput = CurrFsm.CurrState as VirtualInput;
            };
            SetEnable(false);
        }
        internal void OnUpdate()
        {
            CurrFsm.OnUpdate();
        }

        public void SetEnable(bool enable)
        {
            CurrFsm.SetParam(ParamConst.IntIsEnable, enable);
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
}

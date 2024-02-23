using Main;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace YouYou
{
    public enum InputName
    {
        BuyTower,
    }
    public class InputManager : Observable
    {
        //虚拟轴和按钮类-适用于移动输入
        //可以映射到触摸操纵杆，倾斜，陀螺仪等，取决于所需的实现。
        //也可以由其他输入设备实现，如kinect、电子传感器等
        public class VirtualAxis
        {
            public string Name { get; private set; }
            private float m_Value;
            public bool MatchWithInputManager { get; private set; }


            public VirtualAxis(string name) : this(name, true)
            {
            }
            public VirtualAxis(string name, bool matchToInputSettings)
            {
                Name = name;
                MatchWithInputManager = matchToInputSettings;
            }

            public void Update(float value)
            {
                m_Value = value;
            }

            public float GetValue
            {
                get { return m_Value; }
            }

            public float GetValueRaw
            {
                get { return m_Value; }
            }
        }

        //一个控制器游戏对象(例如。一个虚拟GUI按钮)应该调用这个类的'pressed'函数。然后其他对象可以读取
        //该按钮的Get/Down/Up状态
        public class VirtualButton
        {
            public InputName Name { get; private set; }
            public bool MatchWithInputManager { get; private set; }

            private int m_LastPressedFrame = -5;
            private int m_ReleasedFrame = -5;
            private bool m_Pressed;

            public VirtualButton(InputName name) : this(name, true)
            {
            }
            public VirtualButton(InputName name, bool matchToInputSettings)
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

            // these are the states of the button which can be read via the cross platform input system
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
        public enum State
        {
            None,
            Touch,
            KeyboardMouse,
        }
        public Fsm<InputManager> CurrFsm { get; private set; }

        private VirtualInput CurrInput;


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

        public VirtualButton VirtualButtonReference(InputName name)
        {
            return CurrInput.VirtualButtonReference(name);
        }

        // returns the platform appropriate axis for the given name
        public float GetAxis(string name)
        {
            return GetAxis(name, false);
        }
        public float GetAxisRaw(string name)
        {
            return GetAxis(name, true);
        }
        // private function handles both types of axis (raw and not raw)
        private float GetAxis(string name, bool raw)
        {
            return CurrInput.GetAxis(name, raw);
        }

        // -- Button handling --
        public bool GetButton(InputName name)
        {
            return CurrInput.GetButton(name);
        }
        public bool GetButtonDown(InputName name)
        {
            return CurrInput.GetButtonDown(name);
        }
        public bool GetButtonUp(InputName name)
        {
            return CurrInput.GetButtonUp(name);
        }

        public void SetButtonDown(InputName name)
        {
            CurrInput.SetButtonDown(name);
        }
        public void SetButtonUp(InputName name)
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

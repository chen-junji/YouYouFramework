using System;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    public abstract class VirtualInput : FsmState<InputManager>
    {
        protected Dictionary<string, InputManager.VirtualAxis> m_VirtualAxes = new Dictionary<string, InputManager.VirtualAxis>();
        protected Dictionary<InputName, InputManager.VirtualButton> m_VirtualButtons = new Dictionary<InputName, InputManager.VirtualButton>();
        protected List<string> m_AlwaysUseVirtual = new List<string>();


        internal override void OnLeave()
        {
            base.OnLeave();
            foreach (var item in m_VirtualButtons)
            {
                if (item.Value.GetButton || item.Value.GetButtonDown)
                {
                    SetButtonUp(item.Key);
                }
            }
        }

        public bool GetButton(InputName name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButton;
            }

            GameEntry.Input.RegisterVirtualButton(new InputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButton;
        }
        public bool GetButtonDown(InputName name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonDown;
            }

            GameEntry.Input.RegisterVirtualButton(new InputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButtonDown;
        }
        public bool GetButtonUp(InputName name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonUp;
            }

            GameEntry.Input.RegisterVirtualButton(new InputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButtonUp;
        }

        public void SetButtonDown(InputName name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                GameEntry.Input.RegisterVirtualButton(new InputManager.VirtualButton(name));
            }
            m_VirtualButtons[name].Pressed();
        }
        public void SetButtonUp(InputName name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                GameEntry.Input.RegisterVirtualButton(new InputManager.VirtualButton(name));
            }
            m_VirtualButtons[name].Released();
        }

        public void RegisterVirtualAxis(InputManager.VirtualAxis axis)
        {
            if (m_VirtualAxes.ContainsKey(axis.Name))
            {
                YouYouFramework.GameEntry.LogError(LogCategory.Framework, "已经有了一个虚拟轴 " + axis.Name + " 重复注册.");
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


        public void RegisterVirtualButton(InputManager.VirtualButton button)
        {
            if (m_VirtualButtons.ContainsKey(button.Name))
            {
                YouYouFramework.GameEntry.LogError(LogCategory.Framework, "There is already a virtual button named " + button.Name + " registered.");
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

        public InputManager.VirtualAxis VirtualAxisReference(string name)
        {
            if (!m_VirtualAxes.TryGetValue(name, out InputManager.VirtualAxis axis))
            {
                axis = new InputManager.VirtualAxis(name);
                GameEntry.Input.RegisterVirtualAxis(axis);
            }
            return axis;
        }
        public InputManager.VirtualButton VirtualButtonReference(InputName name)
        {
            if (!m_VirtualButtons.TryGetValue(name, out InputManager.VirtualButton button))
            {
                button = new InputManager.VirtualButton(name);
                GameEntry.Input.RegisterVirtualButton(button);
            }
            return button;
        }


        public abstract float GetAxis(string name, bool raw);

        public abstract void SetAxisPositive(string name);
        public abstract void SetAxisNegative(string name);
        public abstract void SetAxisZero(string name);
        public abstract void SetAxis(string name, float value);
    }
}

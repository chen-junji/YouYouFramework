using System;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public abstract class VirtualInput : FsmState<CrossPlatformInputManager>
    {
        protected Dictionary<string, CrossPlatformInputManager.VirtualAxis> m_VirtualAxes = new Dictionary<string, CrossPlatformInputManager.VirtualAxis>();
        protected Dictionary<InputName, CrossPlatformInputManager.VirtualButton> m_VirtualButtons = new Dictionary<InputName, CrossPlatformInputManager.VirtualButton>();
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

            GameEntry.Input.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButton;
        }
        public bool GetButtonDown(InputName name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonDown;
            }

            GameEntry.Input.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButtonDown;
        }
        public bool GetButtonUp(InputName name)
        {
            if (m_VirtualButtons.ContainsKey(name))
            {
                return m_VirtualButtons[name].GetButtonUp;
            }

            GameEntry.Input.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
            return m_VirtualButtons[name].GetButtonUp;
        }

        public void SetButtonDown(InputName name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                GameEntry.Input.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
            }
            m_VirtualButtons[name].Pressed();
        }
        public void SetButtonUp(InputName name)
        {
            if (!m_VirtualButtons.ContainsKey(name))
            {
                GameEntry.Input.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
            }
            m_VirtualButtons[name].Released();
        }

        public void RegisterVirtualAxis(CrossPlatformInputManager.VirtualAxis axis)
        {
            // check if we already have an axis with that name and log and error if we do
            if (m_VirtualAxes.ContainsKey(axis.Name))
            {
                YouYou.GameEntry.LogError(LogCategory.Framework, "There is already a virtual axis named " + axis.Name + " registered.");
            }
            else
            {
                // add any new axes
                m_VirtualAxes.Add(axis.Name, axis);

                // if we dont want to match with the input manager setting then revert to always using virtual
                if (!axis.MatchWithInputManager)
                {
                    m_AlwaysUseVirtual.Add(axis.Name);
                }
            }
        }


        public void RegisterVirtualButton(CrossPlatformInputManager.VirtualButton button)
        {
            // check if already have a buttin with that name and log an error if we do
            if (m_VirtualButtons.ContainsKey(button.Name))
            {
                YouYou.GameEntry.LogError(LogCategory.Framework, "There is already a virtual button named " + button.Name + " registered.");
            }
            else
            {
                // add any new buttons
                m_VirtualButtons.Add(button.Name, button);

                // if we dont want to match to the input manager then always use a virtual axis
                if (!button.MatchWithInputManager)
                {
                    m_AlwaysUseVirtual.Add(button.Name.ToString());
                }
            }
        }

        public CrossPlatformInputManager.VirtualAxis VirtualAxisReference(string name)
        {
            if (!m_VirtualAxes.TryGetValue(name, out CrossPlatformInputManager.VirtualAxis axis))
            {
                axis = new CrossPlatformInputManager.VirtualAxis(name);
                GameEntry.Input.RegisterVirtualAxis(axis);
            }
            return axis;
        }
        public CrossPlatformInputManager.VirtualButton VirtualButtonReference(InputName name)
        {
            if (!m_VirtualButtons.TryGetValue(name, out CrossPlatformInputManager.VirtualButton button))
            {
                button = new CrossPlatformInputManager.VirtualButton(name);
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

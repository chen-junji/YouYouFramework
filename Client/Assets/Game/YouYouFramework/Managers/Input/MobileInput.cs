using System;
using UnityEngine;


namespace YouYouFramework
{
    public class MobileInput : VirtualInput
    {
        internal override void OnEnter()
        {
            base.OnEnter();
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

        public override void SetAxisPositive(string name)
        {
            CheckAddAxes(name);
            m_VirtualAxes[name].Update(1f);
        }
        public override void SetAxisNegative(string name)
        {
            CheckAddAxes(name);
            m_VirtualAxes[name].Update(-1f);
        }
        public override void SetAxisZero(string name)
        {
            CheckAddAxes(name);
            m_VirtualAxes[name].Update(0f);
        }
        public override void SetAxis(string name, float value)
        {
            CheckAddAxes(name);
            m_VirtualAxes[name].Update(value);
        }
    }
}

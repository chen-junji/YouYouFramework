using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouFramework
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
}
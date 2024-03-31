using YouYouFramework;
using UnityEngine;
using System;

public partial class CircleForm : UIFormBase
{
    private void Update()
    {
        m_Trans_Circle.Rotate(0, 0, -5, Space.Self);
    }
}

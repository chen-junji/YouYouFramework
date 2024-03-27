using YouYou;
using UnityEngine;
using System;

public partial class CircleForm : UIFormBase
{
    protected override void Awake()
    {
        base.Awake();
        GetBindComponents(gameObject);
    }
    private void Update()
    {
        m_Trans_Circle.Rotate(0, 0, -5, Space.Self);
    }
}

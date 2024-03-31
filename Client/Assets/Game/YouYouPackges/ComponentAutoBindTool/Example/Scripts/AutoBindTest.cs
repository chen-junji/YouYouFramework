using UnityEngine;
using YouYouFramework;

public partial class AutoBindTest : ComponentAutoBindBase
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("m_Txt_Test3.text==" + m_Txt_Test3.text);
    }
}

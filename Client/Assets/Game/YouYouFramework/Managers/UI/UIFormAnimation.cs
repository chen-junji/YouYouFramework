using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFormAnimation : MonoBehaviour
{
    private void OnEnable()
    {
#if UNITY_EDITOR
        transform.SetAsLastSibling();
#endif
        AnimOpen();
    }


    public void AnimOpen()
    {
        transform.DoShowScale(0.3f, 1);
    }

    public void AnimClose()
    {
        transform.DoShowScale(0.2f, 0);
    }
}

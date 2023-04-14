using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            GameEntry.UI.OpenUIForm<FormDialog>();
        }
    }
}
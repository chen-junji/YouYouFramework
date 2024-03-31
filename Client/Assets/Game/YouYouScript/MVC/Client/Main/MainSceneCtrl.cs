using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class MainSceneCtrl : MonoBehaviour
{
    void Start()
    {
        GameEntry.UI.OpenUIForm<MainForm>();
    }
}

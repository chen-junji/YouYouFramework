using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class MainSceneCtrl : MonoBehaviour
{
    void Start()
    {
        GameEntry.UI.OpenUIForm<MainForm>();
    }
}

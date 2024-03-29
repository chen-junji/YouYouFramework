using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class BtnBack : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameEntry.Log(LogCategory.ZhangSan, "回到上一级场景");
            GameEntry.Scene.UnLoadCurrScene();
            GameEntry.UI.OpenUIForm<MainForm>();
        });
    }
}

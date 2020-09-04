//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestScene : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            GameEntry.Scene.LoadScene(1,true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            GameEntry.Scene.LoadScene(2, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            GameEntry.Scene.LoadScene(3, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            GameEntry.Scene.LoadScene(4, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            GameEntry.Scene.LoadScene(5, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            GameEntry.Scene.LoadScene(6, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad7))
        {
            GameEntry.Scene.LoadScene(7, true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            GameEntry.Scene.LoadScene(8, true);
        }
    }
}
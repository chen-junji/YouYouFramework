using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

public class TestInput : MonoBehaviour
{
    [SerializeField] Button button;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            //手机端, 点击按钮触发Input
            GameEntry.Input.SetButtonUp(InputKey.BuyTower);
        });
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //PC端, 键盘按下A, 触发Input
            GameEntry.Input.SetButtonUp(InputKey.BuyTower);
        }

        if (GameEntry.Input.GetButtonUp(InputKey.BuyTower))
        {
            //监听Input触发, 打印日志
            GameEntry.Log(LogCategory.Normal, InputKey.BuyTower.ToString());
        }
    }
}

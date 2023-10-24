using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestTime : MonoBehaviour
{
    TimeAction action;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            action = GameEntry.Time.CreateTimerLoop(this, 1f, 10, (int loop) =>
            {
                GameEntry.Log(LogCategory.ZhangSan, "运行中 剩余次数=" + loop);
            }, () =>
            {
                GameEntry.Log(LogCategory.ZhangSan, "定时器运行完毕");
            }, false);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            //提前停止
            action.Stop();
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            GameEntry.Time.CreateTimer(this, 1, () =>
            {
                GameEntry.Log(LogCategory.ZhangSan, "延迟1秒等待结束时");
            });
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            Attack();
        }
    }

    private async void Attack()
    {
        GameEntry.Log(LogCategory.ZhangSan, "怪物SetActive(true)");

        await GameEntry.Time.Delay(this, 1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物出生动画播完");

        await GameEntry.Time.Delay(this, 1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物丢炸弹动画播完");

        await GameEntry.Time.Delay(this, 1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物遁地动画播完");
        GameEntry.Log(LogCategory.ZhangSan, "怪物SetActive(false)");
    }
}
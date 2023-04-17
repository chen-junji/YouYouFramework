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
            GameEntry.Log(LogCategory.ZhangSan, "创建了定时器 延迟1秒 间隔1秒 循环100次");
            action = GameEntry.Time.Create(1, () =>
            {
                GameEntry.Log(LogCategory.ZhangSan, "延迟1秒等待结束时");
            }, 1f, 100, (int loop) =>
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
            GameEntry.Log(LogCategory.ZhangSan, "创建了定时器 延迟1秒");
            GameEntry.Time.Create(1, () =>
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

        await GameEntry.Time.Delay(1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物出生动画播完");

        await GameEntry.Time.Delay(1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物丢炸弹动画播完");

        await GameEntry.Time.Delay(1);
        GameEntry.Log(LogCategory.ZhangSan, "怪物遁地动画播完");
        GameEntry.Log(LogCategory.ZhangSan, "怪物SetActive(false)");
    }
}
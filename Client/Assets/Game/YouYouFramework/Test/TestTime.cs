using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestTime : MonoBehaviour
{
    void Start()
    {

    }

    TimeAction action;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            Debug.Log("创建了定时器 延迟1秒 间隔1秒 循环100次");
            action = GameEntry.Time.Create(null, 1, 1f, 100, false, () =>
             {
                 Debug.Log("定时器开始运行");
             }, (int loop) =>
             {
                 Debug.Log("运行中 剩余次数=" + loop);
             }, () =>
             {
                 Debug.Log("定时器运行完毕");
             });

            GameEntry.Time.Create(timeName: "youyou2", delayTime: 10, 1f, 100, onStar: () =>
            {

            }, onUpdate: (int loop) =>
            {

            });
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            //GameEntry.Time.RemoveTimeActionByName("youyou2");

            Attack();
            IEAttack();
        }
    }

    private async void Attack()
    {
        Debug.Log("怪物SetActive(true)");

        await GameEntry.Time.Delay(1);
        Debug.Log("怪物出生动画播完");

        await GameEntry.Time.Delay(1);
        Debug.Log("怪物丢炸弹动画播完");

        await GameEntry.Time.Delay(1);
        Debug.Log("怪物遁地动画播完");
        Debug.Log("怪物SetActive(false)");
    }
    IEnumerator IEAttack()
    {
        Debug.Log("怪物SetActive(true)");

        yield return new WaitForSeconds(1);
        Debug.Log("怪物出生动画播完");

        yield return new WaitForSeconds(1);
        Debug.Log("怪物丢炸弹动画播完");

        yield return new WaitForSeconds(1);
        Debug.Log("怪物遁地动画播完");
        Debug.Log("怪物SetActive(false)");
    }
}
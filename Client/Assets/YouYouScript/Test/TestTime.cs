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
            //创建定时器
            action = GameEntry.Time.CreateTimeAction();

            Debug.Log("创建了定时器 延迟10秒 间隔1秒 循环100次");
            action.Init(timeName: "youyou1", delayTime: 10, 1f, 100, () =>
              {
                  Debug.Log("定时器开始运行");
              }, (int loop) =>
              {
                  Debug.Log("运行中 剩余次数=" + loop);
              }, () =>
              {
                  Debug.Log("定时器运行完毕");
              }).Run();
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            GameEntry.Time.RemoveTimeActionByName("youyou1");
        }
    }
}
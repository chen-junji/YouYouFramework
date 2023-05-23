using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class SceneCtrl : SingletonMono<SceneCtrl>
{
    void Start()
    {
        GameEntry.Log(LogCategory.ZhangSan, "SceneCtrl.Start()");
        FormDialog.ShowForm("框架内部流程全部加载完毕, 已经进入登录流程", "登录流程");
    }
}

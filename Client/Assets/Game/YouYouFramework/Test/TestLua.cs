//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class TestLua : MonoBehaviour
{
    void Start()
    {
        GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.LoadingProgressChange, null);
        GameEntry.Event.CommonEvent.RemoveEventListener(CommonEventId.LoadingProgressChange, null);

        UnityEngine.UI.Scrollbar bar = null;
        bar.value = 1;
        bar.size = 1;

        Button button = null;
        button.onClick.AddListener(() => { });
        
        //GameEntry.Lua.SendHttpData()
    }
}
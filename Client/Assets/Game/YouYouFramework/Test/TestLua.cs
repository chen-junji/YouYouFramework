using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class TestLua : MonoBehaviour
{
    void Start()
    {
        GameEntry.Event.Common.AddEventListener(CommonEventId.LoadingProgressChange, null);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.LoadingProgressChange, null);

        UnityEngine.UI.Scrollbar bar = null;
        bar.value = 1;
        bar.size = 1;

        Button button = null;
        button.onClick.AddListener(() => { });
        
        //GameEntry.Lua.SendHttpData()
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class CircleCtrl : Singleton<CircleCtrl>
{
    private int showCount = 0;

    public void CircleOpen()
    {
        showCount++;
        if (showCount == 1)
        {
            GameEntry.UI.OpenUIForm<CircleForm>();
        }
    }
    public void CircleClose()
    {
        showCount--;
        if (showCount == 0)
        {
            GameEntry.UI.CloseUIForm<CircleForm>();
        }
    }
}

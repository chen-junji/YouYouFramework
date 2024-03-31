using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public partial class GameForm : UIFormBase
{
    internal static void ShowForm()
    {
        GameEntry.UI.OpenUIForm<GameForm>();
    }
}

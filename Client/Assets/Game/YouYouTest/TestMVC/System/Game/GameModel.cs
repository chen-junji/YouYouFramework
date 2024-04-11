using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public event Action TestEvent;

    internal void DispatchTestEvent()
    {
        TestEvent?.Invoke();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public BehaviourTreeSO BTSO;

    private void Start()
    {
        BehaviourTree bt = BTSO.CloneBehaviourTree();
        bt.Start("Debugger Test");
    }
}

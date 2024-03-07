using System;
using UnityEngine;

public class BehaviourTreeComponent : MonoBehaviour
{
    private void Update()
    {
        UpdateManager.OnUpdate(Time.deltaTime);
    }
}
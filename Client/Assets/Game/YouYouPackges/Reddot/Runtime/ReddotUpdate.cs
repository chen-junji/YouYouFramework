using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReddotUpdate : MonoBehaviour
{
    private void Update()
    {
        ReddotManager.Instance.OnUpdate();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestPool : MonoBehaviour
{
    async void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.Player);
        }
    }
}
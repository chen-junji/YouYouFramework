using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestScene : MonoBehaviour
{
    async void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.TestMVC);
        }
    }
}
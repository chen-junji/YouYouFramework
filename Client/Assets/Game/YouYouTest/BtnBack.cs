using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

public class BtnBack : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameEntry.Log("回到上一级场景");
            GameEntry.UI.OpenUIForm<LoadingForm>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Main);
        });
    }
}

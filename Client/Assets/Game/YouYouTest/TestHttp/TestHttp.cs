using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;
using UnityEngine.Networking;


/// <summary>
/// 备注: 在Unity菜单栏/YouYouTool/YouYouEditor/ParamsSettings内,配置了Http的请求路径, 如:127.0.0.1:8083
/// 备注2:如果没有后端给你提供接口, 这个方法是无法调用的, 看看代码范例就好 
/// </summary>
public class TestHttp : MonoBehaviour
{
    async void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //Get请求, 自动判断HasError
            GameEntry.Http.Get("Test/AAA", callBack: (string json) =>
            {
                GameEntry.Log(LogCategory.Normal, json);
            });

        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            //Get请求
            GameEntry.Http.GetArgs("Test/AAA", callBack: (callBackArgs) =>
            {
                if (callBackArgs.result == UnityWebRequest.Result.Success)
                {
                    GameEntry.Log(LogCategory.Normal, callBackArgs.downloadHandler.text);
                }
            });
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            //Post请求
            GameEntry.Http.Post("Test/AAA", callBack: (string json) =>
            {
                GameEntry.Log(LogCategory.Normal, json);
            });
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            //Post请求 
            string json = await GameEntry.Http.PostAsync("Test/AAA");
            GameEntry.Log(LogCategory.Normal, json);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            //Post请求, 自动弹出转圈UI, 屏蔽玩家点击
            string json = await GameEntry.Http.PostAsync("Test/AAA", loadingCircle: true);
            GameEntry.Log(LogCategory.Normal, json);
        }
    }
}

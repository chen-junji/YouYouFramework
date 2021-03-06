//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YouYou;

public class TestAsync : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //GameEntry.Data.UserDataManager.ShareUserData.CurrRoleId = 100;
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {

        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            //Debug.LogError(GameEntry.Data.UserDataManager.ShareUserData.AccountId);
        }
        //if (Input.GetKeyUp(KeyCode.X))
        //{
        //    //Debug.Log("执行同步方法");
        //    //TestMethod();


        //    //关键词 Task async await
        //    Debug.Log("执行异步方法");
        //    //第一种方式 也是常用的方式
        //    //Task.Factory.StartNew(TestMethod);

        //    //第二种方式
        //    TestMethodAsync();
        //    Debug.Log("执行异步方法结束");

        //}
    }

    public async void TestMethodAsync()
    {
        int result = await Test1();

        Debug.Log("方法结果=" + result);
    }

    public async Task<int> Test1()
    {
        int ret = 0;
        for (int i = 0; i < 100; i++)
        {
            ret += i;
            await Task.Delay(1);
        }
        return ret;
    }

    //private void TestMethod()
    //{
    //    for (int i = 0; i < 20000; i++)
    //    {
    //        Debug.Log(i);
    //    }
    //}
}
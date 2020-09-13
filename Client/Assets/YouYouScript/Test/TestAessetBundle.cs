//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYou;

public class TestAessetBundle : MonoBehaviour
{

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
        {
            //GameEntry.Pool.GameObjectSpawn
            GameEntry.Pool.GameObjectSpawn(1, null);
        }
    }
}
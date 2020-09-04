//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestDownload : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();

            GameEntry.Download.BeginDownloadSingle("download/datatable.assetbundle");

            //LinkedList<string> lst = new LinkedList<string>();

            //lst.AddLast("download/cusshaders.assetbundle");
            //lst.AddLast("download/datatable.assetbundle");
            //lst.AddLast("download/xlualogic.assetbundle");
            //lst.AddLast("download/effect/effectsources/animation/camera001.assetbundle");
            //lst.AddLast("download/effect/effectsources/animation/camera002.assetbundle");
            //lst.AddLast("download/effect/effectsources/animation/camera003.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/circle02_3m.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/cone02.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/cylinder_20pl.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/duanzao.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/fu_001_001.assetbundle");
            //lst.AddLast("download/effect/effectsources/model/fu_01.assetbundle");

            //GameEntry.Download.BeginDownloadMulit(lst, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
    }

    private void OnDownloadMulitComplete()
    {
        Debug.LogError("下载完毕");
    }

    private void OnDownloadMulitUpdate(int t1, int t2, ulong t3, ulong t4)
    {
        Debug.LogError(string.Format("下载中 当前数量{0}/{1}  当前大小(字节){2}/{3}", t1, t2, t3, t4));
    }
}
using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestDownload : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //这是为了删掉断点续传存档点
            PlayerPrefs.DeleteAll();

            //单文件下载
            MainEntry.Download.BeginDownloadSingle("download/datatable.assetbundle");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            //这是为了删掉断点续传存档点
            PlayerPrefs.DeleteAll();

            //多文件下载
            LinkedList<string> lst = new LinkedList<string>();
            lst.AddLast("download/cusshaders.assetbundle");
            lst.AddLast("download/datatable.assetbundle");
            lst.AddLast("download/effect/effectsources/model/fu_01.assetbundle");
            MainEntry.Download.BeginDownloadMulit(lst, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
    }
    private void OnDownloadMulitComplete()
    {
        GameEntry.Log(LogCategory.ZhangSan, "下载完毕");
    }
    private void OnDownloadMulitUpdate(int t1, int t2, ulong t3, ulong t4)
    {
        GameEntry.Log(LogCategory.ZhangSan, string.Format("下载中 当前数量{0}/{1}  当前大小(字节){2}/{3}", t1, t2, t3, t4));
    }
}
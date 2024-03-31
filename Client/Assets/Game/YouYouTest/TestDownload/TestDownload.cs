using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// 备注: 如果你没有打AssetBundle包并部署资源, 是无法下载的, 看看代码范例就好
/// </summary>
public class TestDownload : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            //这是为了删掉断点续传存档点
            PlayerPrefs.DeleteAll();

            //单文件下载
            MainEntry.Download.BeginDownloadSingle("download/datatable.assetbundle");
        }

        if (Input.GetKeyUp(KeyCode.S))
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
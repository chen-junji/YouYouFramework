using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;
using Main;

/// <summary>
/// 这个界面脚本不做热更, 所以不继承UIFormBase
/// </summary>
public class FormCheckVersion : MonoBehaviour
{
    [SerializeField]
    private Text txtTip;
    //[SerializeField]
    //private Text txtSize;
    //[SerializeField]
    //private Text txtVersion;

    [SerializeField]
    private Scrollbar scrollbar;

    private void OnDestroy()
    {
        MainEntry.ResourceManager.CheckVersionBeginDownload -= OnCheckVersionBeginDownload;
        MainEntry.ResourceManager.CheckVersionDownloadUpdate -= OnCheckVersionDownloadUpdate;
        MainEntry.ResourceManager.CheckVersionDownloadComplete -= OnCheckVersionDownloadComplete;

        MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.PRELOAD_BEGIN, OnPreloadBegin);
        MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.PRELOAD_UPDATE, OnPreloadUpdate);
        MainEntry.Data.RemoveEventListener(SysDataMgr.EventName.PRELOAD_COMPLETE, OnPreloadComplete);
    }
    private void Start()
    {
        MainEntry.ResourceManager.CheckVersionBeginDownload += OnCheckVersionBeginDownload;
        MainEntry.ResourceManager.CheckVersionDownloadUpdate += OnCheckVersionDownloadUpdate;
        MainEntry.ResourceManager.CheckVersionDownloadComplete += OnCheckVersionDownloadComplete;

        MainEntry.Data.AddEventListener(SysDataMgr.EventName.PRELOAD_BEGIN, OnPreloadBegin);
        MainEntry.Data.AddEventListener(SysDataMgr.EventName.PRELOAD_UPDATE, OnPreloadUpdate);
        MainEntry.Data.AddEventListener(SysDataMgr.EventName.PRELOAD_COMPLETE, OnPreloadComplete);

        //if (txtSize != null) txtSize.gameObject.SetActive(false);
    }

    #region 检查更新进度
    private void OnCheckVersionBeginDownload()
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(true);

        //txtVersion.text = string.Format("最新版本 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    private void OnCheckVersionDownloadUpdate(BaseParams baseParams)
    {
        txtTip.text = string.Format("正在下载{0}/{1}", baseParams.IntParam1, baseParams.IntParam2);
        //if (txtSize != null) txtSize.text = string.Format("{0:f2}M/{1:f2}M", (float)baseParams.ULongParam1 / (1024 * 1024), (float)baseParams.ULongParam2 / (1024 * 1024));

        scrollbar.size = (float)baseParams.IntParam1 / baseParams.IntParam2;
    }
    private void OnCheckVersionDownloadComplete()
    {
        //Debug.Log("检查更新下载完毕!!!");
    }
    #endregion

    #region 预加载进度
    private void OnPreloadComplete(object userData)
    {
        Destroy(gameObject);
    }
    private void OnPreloadUpdate(object userData)
    {
        float baseParams = (float)userData;

        txtTip.text = string.Format("正在加载资源{0:f0}%", baseParams);

        scrollbar.size = baseParams * 0.01f;
    }
    private void OnPreloadBegin(object userData)
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(false);
        //txtVersion.text = string.Format("资源版本号 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    #endregion
}

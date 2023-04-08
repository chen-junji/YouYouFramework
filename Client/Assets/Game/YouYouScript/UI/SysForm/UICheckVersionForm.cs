using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class UICheckVersionForm : MonoBehaviour
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
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.CheckVersionBeginDownload, OnCheckVersionBeginDownload);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.CheckVersionDownloadUpdate, OnCheckVersionDownloadUpdate);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.CheckVersionDownloadComplete, OnCheckVersionDownloadComplete);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.PreloadBegin, OnPreloadBegin);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.PreloadUpdate, OnPreloadUpdate);
        GameEntry.Event.Common.RemoveEventListener(CommonEventId.PreloadComplete, OnPreloadComplete);
    }
    private void Start()
    {
        GameEntry.Event.Common.AddEventListener(CommonEventId.CheckVersionBeginDownload, OnCheckVersionBeginDownload);
        GameEntry.Event.Common.AddEventListener(CommonEventId.CheckVersionDownloadUpdate, OnCheckVersionDownloadUpdate);
        GameEntry.Event.Common.AddEventListener(CommonEventId.CheckVersionDownloadComplete, OnCheckVersionDownloadComplete);
        GameEntry.Event.Common.AddEventListener(CommonEventId.PreloadBegin, OnPreloadBegin);
        GameEntry.Event.Common.AddEventListener(CommonEventId.PreloadUpdate, OnPreloadUpdate);
        GameEntry.Event.Common.AddEventListener(CommonEventId.PreloadComplete, OnPreloadComplete);

        //if (txtSize != null) txtSize.gameObject.SetActive(false);
    }

    #region 检查更新进度
    private void OnCheckVersionBeginDownload(object userData)
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(true);

        //txtVersion.text = string.Format("最新版本 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    private void OnCheckVersionDownloadUpdate(object userData)
    {
        BaseParams baseParams = userData as BaseParams;

        txtTip.text = string.Format("正在下载{0}/{1}", baseParams.IntParam1, baseParams.IntParam2);
        //if (txtSize != null) txtSize.text = string.Format("{0:f2}M/{1:f2}M", (float)baseParams.ULongParam1 / (1024 * 1024), (float)baseParams.ULongParam2 / (1024 * 1024));

        scrollbar.size = (float)baseParams.IntParam1 / baseParams.IntParam2;
    }
    private void OnCheckVersionDownloadComplete(object userData)
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
        BaseParams baseParams = userData as BaseParams;

        txtTip.text = string.Format("正在加载资源{0:f0}%", baseParams.FloatParam1);

        scrollbar.size = baseParams.FloatParam1 * 0.01f;
    }
    private void OnPreloadBegin(object userData)
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(false);
        //txtVersion.text = string.Format("资源版本号 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    #endregion
}

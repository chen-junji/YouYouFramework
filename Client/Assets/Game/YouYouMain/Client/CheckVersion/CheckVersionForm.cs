using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using YouYouMain;

/// <summary>
/// 这个界面脚本不做热更, 所以不继承UIFormBase
/// </summary>
public class CheckVersionForm : MonoBehaviour
{
    [SerializeField]
    private Text txtTip;
    //[SerializeField]
    //private Text txtSize;
    //[SerializeField]
    //private Text txtVersion;

    [SerializeField]
    private Image progressImg;

    private void OnDestroy()
    {
        CheckVersionCtrl.Instance.CheckVersionBeginDownload -= OnCheckVersionBeginDownload;
        CheckVersionCtrl.Instance.CheckVersionDownloadUpdate -= OnCheckVersionDownloadUpdate;
        CheckVersionCtrl.Instance.CheckVersionDownloadComplete -= OnCheckVersionDownloadComplete;

        MainEntry.Instance.ActionPreloadBegin -= OnPreloadBegin;
        MainEntry.Instance.ActionPreloadUpdate -= OnPreloadUpdate;
        MainEntry.Instance.ActionPreloadComplete -= OnPreloadComplete;
    }
    private void Start()
    {
        CheckVersionCtrl.Instance.CheckVersionBeginDownload += OnCheckVersionBeginDownload;
        CheckVersionCtrl.Instance.CheckVersionDownloadUpdate += OnCheckVersionDownloadUpdate;
        CheckVersionCtrl.Instance.CheckVersionDownloadComplete += OnCheckVersionDownloadComplete;

        MainEntry.Instance.ActionPreloadBegin += OnPreloadBegin;
        MainEntry.Instance.ActionPreloadUpdate += OnPreloadUpdate;
        MainEntry.Instance.ActionPreloadComplete += OnPreloadComplete;

        //if (txtSize != null) txtSize.gameObject.SetActive(false);
    }

    #region 检查更新进度
    private void OnCheckVersionBeginDownload()
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(true);

        //txtVersion.text = string.Format("最新版本 {0}", GameEntry.Loader.ResourceManager.CDNVersion);
    }
    private void OnCheckVersionDownloadUpdate(DownloadStatus status)
    {
        //status.Percent; // 进度（0~1）
        //status.DownloadedBytes 当前下载字节
        //status.TotalBytes 最大下载字节

        // 计算下载速度(每秒下载多少)
        //long speedBytesPerSec = (long)(status.DownloadedBytes / Time.deltaTime);
        //string speed = FormatBytes(speedBytesPerSec);
        //_statusText.text = $"{speed}/s";

        txtTip.text = string.Format("{0:f2}M/{1:f2}M", (float)status.DownloadedBytes / (1024 * 1024), status.TotalBytes / (1024 * 1024));
        progressImg.fillAmount = status.Percent;
    }
    private void OnCheckVersionDownloadComplete()
    {
        //Debug.Log("检查更新下载完毕!!!");
    }

    // 字节量格式化（如 B、KB、MB）
    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:F2} {suffixes[i]}";
    }
    #endregion

    #region 预加载进度
    private void OnPreloadComplete()
    {
        Destroy(gameObject);
    }
    private void OnPreloadUpdate(float baseParams)
    {
        txtTip.text = string.Format("正在加载资源{0:f0}%", baseParams * 100);

        progressImg.fillAmount = baseParams;
    }
    private void OnPreloadBegin()
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(false);
        //txtVersion.text = string.Format("资源版本号 {0}", GameEntry.Loader.ResourceManager.CDNVersion);
    }
    #endregion
}

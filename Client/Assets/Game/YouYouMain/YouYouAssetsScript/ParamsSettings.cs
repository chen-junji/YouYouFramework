using YouYouMain;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "YouYouAsset/ParamsSettings")]
public class ParamsSettings : ScriptableObject
{
    #region Http请求相关
    [BoxGroup("Http请求相关")]
    [LabelText("正式服请求路径")]
    public string WebAccountUrl;

    [BoxGroup("Http请求相关")]
    [LabelText("测试服请求路径")]
    public string TestWebAccountUrl;

    [BoxGroup("Http请求相关")]
    [LabelText("当前是否测试服")]
    public bool IsTest;

    [BoxGroup("Http请求相关")]
    [LabelText("是否加密")]
    public bool PostIsEncrypt;

    [BoxGroup("Http请求相关")]
    [LabelText("设置Post的ContentType")]
    public string PostContentType;
    #endregion

    #region 系统参数
    /// <summary>
    /// Http请求的重试次数
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("Http请求的重试次数")]
    public int HttpRetry = 3;

    /// <summary>
    /// Http请求的重试间隔
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("Http请求的重试间隔")]
    public int HttpRetryInterval = 3;

    /// <summary>
    /// 下载请求的重试次数
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("下载请求的重试次数")]
    public int DownloadRetry = 3;
    /// <summary>
    /// 下载多文件器的数量
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("下载多文件器的数量")]
    public int DownloadRoutineCount = 3;
    /// <summary>
    /// 断点续传的存储间隔缓存
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("断点续传的存储间隔缓存")]
    public int DownloadFlushSize = 2048;

    /// <summary>
    /// 类对象池_释放间隔
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("类对象池_释放间隔")]
    public int PoolReleaseClassObjectInterval = 30;
    /// <summary>
    /// AssetBundle池_释放间隔
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("AssetBundle池_释放间隔")]
    public int PoolReleaseAssetBundleInterval = 30;
    /// <summary>
    /// Asset池_释放间隔
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("Asset池_释放间隔")]
    public int PoolReleaseAssetInterval = 60;

    /// <summary>
    /// UI界面池_回池后过期时间_秒
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("UI界面池_回池后过期时间_秒")]
    public int UIExpire = 30;

    /// <summary>
    /// UI界面池_释放间隔_秒
    /// </summary>
    [BoxGroup("系统参数")]
    [LabelText("UI界面池_释放间隔_秒")]
    public int UIClearInterval = 30;
    #endregion

    #region 业务系统测试
    /// <summary>
    /// 是否测试手机端Input系统
    /// </summary>
    [BoxGroup("业务系统测试")]
    [LabelText("是否测试手机端Input系统")]
    public bool MobileDebug;

    /// <summary>
    /// 是否激活新手引导
    /// </summary>
    [BoxGroup("业务系统测试")]
    [LabelText("是否激活新手引导")]
    public bool ActiveGuide;
    #endregion

}
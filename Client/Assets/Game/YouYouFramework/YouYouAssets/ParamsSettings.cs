using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "YouYouAsset/ParamsSettings")]
public class ParamsSettings : ScriptableObject
{
    #region Http请求相关
    [Header("正式服请求路径")]
    public string WebAccountUrl;

    [Header("测试服请求路径")]
    public string TestWebAccountUrl;

    [Header("当前是否测试服")]
    public bool IsTest;

    [Header("是否加密")]
    public bool PostIsEncrypt;

    [Header("设置Post的ContentType")]
    public string PostContentType;
    #endregion

    #region 系统参数

    [Header("Http请求的重试次数")]
    public int HttpRetry = 3;

    [Header("Http请求的重试间隔")]
    public int HttpRetryInterval = 3;

    [Header("类对象池_释放间隔")]
    public int PoolReleaseClassObjectInterval = 30;

    [Header("UI界面池_回池后过期时间_秒")]
    public int UIExpire = 30;

    [Header("UI界面池_释放间隔_秒")]
    public int UIClearInterval = 30;
    #endregion

}
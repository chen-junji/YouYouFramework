using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 渠道配置
/// </summary>
public class ChannelConfigEntity
{
    /// <summary>
    /// 渠道号
    /// </summary>
    public short ChannelId = 146;

    /// <summary>
    /// 服务器时间
    /// </summary>
    public long ServerTime;

    /// <summary>
    /// 资源版本号
    /// </summary>
    public string SourceVersion = "1.0.0";

    /// <summary>
    /// 资源地址
    /// </summary>
    public string SourceUrl = "http://127.0.0.1:8082/";

    #region RealSourceUrl 真正的资源地址
    private string m_RealSourceUrl;
    /// <summary>
    /// 真正的资源地址
    /// </summary>
    public string RealSourceUrl
    {
        get
        {
            if (string.IsNullOrEmpty(m_RealSourceUrl))
            {
                string buildTarget = string.Empty;

#if UNITY_STANDALONE_WIN
                buildTarget = "Windows";
#elif UNITY_ANDROID
				buildTarget = "Android";
#elif UNITY_IPHONE
                buildTarget = "iOS";
#endif
                m_RealSourceUrl = string.Format("{0}{1}/{2}/", SourceUrl, SourceVersion, buildTarget);
            }
            return m_RealSourceUrl;
        }
    }

    #endregion
}

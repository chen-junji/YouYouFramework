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
    /// 资源站点地址
    /// </summary>
    public string SourceUrl = "http://127.0.0.1:9090/ServerData";

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
                m_RealSourceUrl = string.Format("{0}/{1}", SourceUrl, SourceVersion);
            }
            return m_RealSourceUrl;
        }
    }

    #endregion
}

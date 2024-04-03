using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChannelModel
{
    public static ChannelModel Instance { get; private set; } = new ChannelModel();

    /// <summary>
    /// 渠道配置数据
    /// </summary>
    public ChannelConfigEntity CurrChannelConfig { get; private set; }

    /// <summary>
    /// 用于计算时间戳的本地服务器时间
    /// </summary>
    public long CurrServerTime
    {
        get
        {
            if (CurrChannelConfig == null) return (long)Time.unscaledTime;
            return CurrChannelConfig.ServerTime + (long)Time.unscaledTime;
        }
    }

    public ChannelModel()
    {
        CurrChannelConfig = new ChannelConfigEntity();
    }
}
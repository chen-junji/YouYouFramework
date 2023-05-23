using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SysDataMgr : DataMgrBase<SysDataMgr.EventName>
    {
        public enum EventName : uint
        {
            LOADING_SCENE_UPDATE,

            PRELOAD_BEGIN,
            PRELOAD_UPDATE,
            PRELOAD_COMPLETE,
        }

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

        public SysDataMgr()
        {
            CurrChannelConfig = new ChannelConfigEntity();
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SystemCtrl
    {
        private static SystemCtrl instance;
        public static SystemCtrl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SystemCtrl();
                }
                return instance;
            }
        }
    }

    public class SystemModel
    {
        private static SystemModel instance;
        public static SystemModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SystemModel();
                }
                return instance;
            }
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

        public SystemModel()
        {
            CurrChannelConfig = new ChannelConfigEntity();
        }
    }
}
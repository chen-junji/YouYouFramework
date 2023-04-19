using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SysDataMgr
    {
        /// <summary>
        /// 渠道配置数据
        /// </summary>
        public ChannelConfigEntity CurrChannelConfig { get; private set; }

        /// <summary>
        /// Http调用失败后重试次数
        /// </summary>
        public int HttpRetry { get; private set; }
        /// <summary>
        /// Http调用失败后重试间隔（秒）
        /// </summary>
        public int HttpRetryInterval { get; private set; }

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

            HttpRetry = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_Retry, MainEntry.CurrDeviceGrade);
            HttpRetryInterval = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_RetryInterval, MainEntry.CurrDeviceGrade);
        }
    }
}
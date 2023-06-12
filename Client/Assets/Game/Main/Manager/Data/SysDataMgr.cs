using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SysDataMgr 
    {
        public event Action<float> ActionLoadingSceneUpdate;

        public event Action ActionPreloadBegin;
        public event Action<float> ActionPreloadUpdate;
        public event Action ActionPreloadComplete;

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

        public void LoadingSceneUpdate(float progress)
        {
            ActionLoadingSceneUpdate?.Invoke(progress);
        }

        public void PreloadBegin()
        {
            ActionPreloadBegin?.Invoke();
        }
        public void PreloadUpdate(float progress)
        {
            ActionPreloadUpdate?.Invoke(progress);
        }
        public void PreloadComplete()
        {
            ActionPreloadComplete?.Invoke();
        }

    }
}
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
            //场景加载_进度更新
            LOADING_SCENE_UPDATE,

            //预加载_开始
            PRELOAD_BEGIN,
            //预加载_进度更新
            PRELOAD_UPDATE,
            //预加载_完毕
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


        public void LoadingSceneUpdate(float progress)
        {
            BaseParams m_CurrLoadingParam = MainEntry.ClassObjectPool.Dequeue<BaseParams>();
            m_CurrLoadingParam.FloatParam1 = progress;
            Dispatch(SysDataMgr.EventName.LOADING_SCENE_UPDATE, m_CurrLoadingParam);
            m_CurrLoadingParam.Reset();
            MainEntry.ClassObjectPool.Enqueue(m_CurrLoadingParam);
        }

        public void PreloadUpdate(float progress)
        {
            BaseParams m_PreloadParams = MainEntry.ClassObjectPool.Dequeue<BaseParams>();
            m_PreloadParams.FloatParam1 = progress;
            Dispatch(EventName.PRELOAD_UPDATE, m_PreloadParams);
            m_PreloadParams.Reset();
            MainEntry.ClassObjectPool.Enqueue(m_PreloadParams);
        }
    }
}
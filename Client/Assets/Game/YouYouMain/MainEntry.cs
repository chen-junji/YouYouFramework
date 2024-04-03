using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YouYouMain
{
    public class MainEntry : MonoBehaviour
    {
        //全局参数设置
        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private ParamsSettings m_ParamsSettings;
        public static ParamsSettings ParamsSettings { get; private set; }

        //预加载相关事件
        public event Action ActionPreloadBegin;
        public event Action<float> ActionPreloadUpdate;
        public event Action ActionPreloadComplete;

        /// <summary>
        /// 下载管理器
        /// </summary>
        public static DownloadManager Download { get; private set; }
        /// <summary>
        /// 检查更新管理器
        /// </summary>
        public static CheckVersionManager CheckVersion { get; private set; }
        /// <summary>
        /// 类对象池
        /// </summary>
        public static ClassObjectPool ClassObjectPool { get; private set; }

        /// <summary>
        /// 代码热更新管理器
        /// </summary>
        public static HotfixManager Hotfix { get; private set; }

        /// <summary>
        /// 当前是否为AssetBundle模式
        /// </summary>
        public static bool IsAssetBundleMode { get; private set; }

        public static MainEntry Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            ParamsSettings = m_ParamsSettings;

#if ASSETBUNDLE
            IsAssetBundleMode = true;
#endif
        }
        private void Start()
        {
            //初始化管理器
            Download = new DownloadManager();
            CheckVersion = new CheckVersionManager();
            ClassObjectPool = new ClassObjectPool();
            Hotfix = new HotfixManager();

            Download.Init();
            CheckVersion.Init();
            Hotfix.Init();
        }
        private void Update()
        {
            Download.OnUpdate();
            ClassObjectPool.OnUpdate();
        }
        private void OnApplicationQuit()
        {
            Download.Dispose();
        }

        internal static void Log(params object[] args)
        {
#if DEBUG_LOG_NORMAL
            //由于性能原因，如果在Build Settings中没有勾上“Development Build”
            //即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            if (!Debug.isDebugBuild)
            {
                return;
            }
            Debug.Log("MainEntryLog - " + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
        }

        internal static void LogWarning(params object[] args)
        {
#if DEBUG_LOG_WARNING
            Debug.LogWarning("MainEntryLog - " + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
        }

        internal static void LogError(params object[] args)
        {
#if DEBUG_LOG_ERROR
            Debug.LogError("MainEntryLog - " + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
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
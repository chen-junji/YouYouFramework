using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class MainEntry : MonoBehaviour
    {
        /// <summary>
        /// 日志分类
        /// </summary>
        public enum LogCategory
        {
            /// <summary>
            /// 框架日志
            /// </summary>
            Framework,
            /// <summary>
            /// 流程
            /// </summary>
            Procedure,
            /// <summary>
            /// 资源管理
            /// </summary>
            Assets,
            /// <summary>
            /// 网络消息
            /// </summary>
            NetWork
        }

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
        }
        private void OnApplicationQuit()
        {
            Download.Dispose();
        }

        internal static void Log(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_NORMAL
            //由于性能原因，项目正式上线后， 即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            if (!Debug.isDebugBuild)
            {
                return;
            }
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.Log(string.Format("MainEntryLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        internal static void LogWarning(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_WARNING
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogWarning(string.Format("MainEntryLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        internal static void LogError(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_ERROR
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogError(string.Format("MainEntryLog=={0}=={1}", catetory.ToString(), value));
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
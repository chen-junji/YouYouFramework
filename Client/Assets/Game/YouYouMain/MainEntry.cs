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

            Download.Init();

            if (IsAssetBundleMode)
            {
                VersionLocalModel.Instance.SetAssetVersion(null);//不检测版本号, 而是直接检测MD5

                //资源检查更新
                CheckVersionCtrl.Instance.CheckVersionChange(() =>
                {
                    //加载Hotfix代码(HybridCLR)
                    HotfixCtrl hotfixCtrl = new HotfixCtrl();
                    hotfixCtrl.LoadHotifx(() =>
                    {
                        //启动YouYouFramework框架入口
                        GameObject gameEntry = hotfixCtrl.hotfixAb.LoadAsset<GameObject>("gameentry.prefab");
                        Instantiate(gameEntry);
                    });
                });
            }
            else
            {
#if UNITY_EDITOR
                //启动YouYouFramework框架入口
                GameObject gameEntry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Download/Hotfix/GameEntry.prefab");
                Instantiate(gameEntry);
#endif
            }

        }
        private void Update()
        {
            Download.OnUpdate();
        }
        private void OnApplicationQuit()
        {
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

    }
}
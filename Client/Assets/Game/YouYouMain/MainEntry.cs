using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace YouYouMain
{
    public class MainEntry : MonoBehaviour
    {
        [BoxGroup("系统参数")]
        [LabelText("单机模式 不检查热更新 资源从StreamingAssets加载")]
        public bool Standalone = false;

        [BoxGroup("系统参数")]
        [LabelText("下载请求的重试次数")]
        public int DownloadRetry = 3;

        [BoxGroup("系统参数")]
        [LabelText("下载器的数量")]
        public int DownloadRoutineCount = 3;

        [BoxGroup("系统参数")]
        [LabelText("断点续传的存储间隔缓存")]
        public int DownloadFlushSize = 2048;

        [BoxGroup("系统参数")]
        [LabelText("是否启用断点续传? (部分CDN不兼容,可能会导致闪退)")]
        public bool IsDownloadFlush = false;

        //预加载相关事件
        public Action ActionPreloadBegin;
        public Action<float> ActionPreloadUpdate;
        public Action ActionPreloadComplete;

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

#if ASSETBUNDLE
            IsAssetBundleMode = true;
#endif
        }
        private async void Start()
        {
            //初始化管理器
            Download = new DownloadManager();

            if (IsAssetBundleMode)
            {
                //初始化本地的版本信息文件
                await CheckVersionCtrl.Instance.Init();

                if (MainEntry.Instance.Standalone == false)
                {
                    //这里不比对总版本号, 而是直接遍历所有AB包的MD5进行比对, 如果要比对总版本号, 就把这句代码注释掉
                    VersionLocalModel.Instance.SetAssetVersion("");
                    VersionStreamingModel.Instance.AssetVersion = "";

                    //检查更新
                    CheckVersionCtrl.Instance.CheckVersionChange(() =>
                    {
                        //检查更新完成, 加载Hotfix代码(HybridCLR)
                        HotfixCtrl.Instance.LoadHotifx();

                        //启动YouYouFramework框架入口
                        LoadGameEntryAB();
                    });
                }
                else
                {
                    //单机模式, 不检查热更新
                    //加载Hotfix代码(HybridCLR)
                    HotfixCtrl.Instance.LoadHotifx();

                    //启动YouYouFramework框架入口
                    LoadGameEntryAB();
                }

            }
            else
            {
#if UNITY_EDITOR
                //启动YouYouFramework框架入口
                GameObject gameEntry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Download/Prefab/GameEntry.prefab");
                Instantiate(gameEntry);
#endif
            }

        }
        private void Update()
        {
            Download.OnUpdate();
        }

        private AssetBundle LoadGameEntryAB()
        {
            VersionFileEntity versionFileEntity = VersionLocalModel.Instance.GetVersionFileEntity(MainConstDefine.GameEntryAssetBundlePath);
            if (versionFileEntity != null)
            {
                //从可写区加载程序集
                AssetBundle gameEntryAb = AssetBundle.LoadFromFile(Path.Combine(MainConstDefine.LocalAssetBundlePath, MainConstDefine.GameEntryAssetBundlePath));
                UnityEngine.Object gameEntryAsset = gameEntryAb.LoadAsset<GameObject>("gameentry.prefab");
                Instantiate(gameEntryAsset);
                return gameEntryAb;
            }

            versionFileEntity = VersionStreamingModel.Instance.GetVersionFileEntity(MainConstDefine.GameEntryAssetBundlePath);
            if (versionFileEntity != null)
            {
                //从只读区加载程序集
                AssetBundle gameEntryAb = AssetBundle.LoadFromFile(Path.Combine(MainConstDefine.StreamingAssetBundlePath, MainConstDefine.GameEntryAssetBundlePath));
                UnityEngine.Object gameEntryAsset = gameEntryAb.LoadAsset<GameObject>("gameentry.prefab");
                Instantiate(gameEntryAsset);
                return gameEntryAb;
            }

            MainEntry.LogError("gameEntryAb不存在");
            return null;
        }

        internal static void Log(object message)
        {
#if DEBUG_LOG_NORMAL
            //由于性能原因，如果在Build Settings中没有勾上“Development Build”
            //即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            if (!Debug.isDebugBuild)
            {
                return;
            }
            Debug.Log($"MainEntryLog==>{message}");
#endif
        }
        internal static void LogWarning(object message)
        {
#if DEBUG_LOG_WARNING
            Debug.LogWarning($"MainEntryLog==>{message}");
#endif
        }
        internal static void LogError(object message)
        {
#if DEBUG_LOG_ERROR
            Debug.LogError($"MainEntryLog==>{message}");
#endif
        }

    }
}
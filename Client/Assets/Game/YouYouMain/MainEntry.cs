using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace YouYouMain
{
    public class MainEntry : MonoBehaviour
    {
        //预加载相关事件
        public Action ActionPreloadBegin;
        public Action<float> ActionPreloadUpdate;
        public Action ActionPreloadComplete;

        public static MainEntry Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            //开始检查更新
            CheckVersionCtrl.Instance.CheckVersionChange(async () =>
            {
                //检查更新完成, 加载Hotfix代码(HybridCLR)
                HotfixCtrl.Instance.LoadHotifx();

                //启动YouYouFramework框架入口
                GameObject gameEntryAsset = await Addressables.LoadAssetAsync<GameObject>("Assets/Game/Download/Prefab/GameEntry.prefab");
                Instantiate(gameEntryAsset);
            });

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
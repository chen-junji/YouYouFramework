using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYouMain
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;
        public static List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib",
            "System",
            "System.Core",
            "UniTask",
            "UnityEngine.CoreModule",
        };

        public HotfixManager()
        {
            //这里防止热更工程找不到AOT工程的类
            System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
            System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;
        }
        public void Init()
        {
            if (MainEntry.IsAssetBundleMode)
            {
                //初始化CDN的VersionFile信息
                MainEntry.CheckVersion.VersionFile.InitCDNVersionFile(() =>
                {
                    //下载并加载热更程序集
                    CheckAndDownload(YFConstDefine.HotfixAssetBundlePath, (string fileUrl) =>
                    {
                        hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
#if !UNITY_EDITOR
                        LoadMetadataForAOTAssemblies();
                        System.Reflection.Assembly.Load(hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes").bytes);
                        MainEntry.Log(MainEntry.LogCategory.Assets, "Assembly-CSharp.dll加载完毕");
#endif
                        UnityEngine.Object.Instantiate(hotfixAb.LoadAsset<GameObject>("gameentry.prefab"));
                    });
                });
            }
            else
            {
#if UNITY_EDITOR
                GameObject gameEntry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Download/Hotfix/GameEntry.prefab");
                UnityEngine.Object.Instantiate(gameEntry);
#endif
            }
        }

        private void CheckAndDownload(string url, Action<string> onComplete)
        {
            bool isEquals = MainEntry.CheckVersion.CheckVersionChangeSingle(url);
            if (isEquals)
            {
                MainEntry.Log(MainEntry.LogCategory.Assets, "资源没变化, 不用重新下载, url==" + url);
                onComplete?.Invoke(url);
            }
            else
            {
                MainEntry.Log(MainEntry.LogCategory.Assets, "资源有更新, 重新下载, url==" + url);
                MainEntry.Download.BeginDownloadSingle(url, onComplete: onComplete);
            }
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = hotfixAb.LoadAsset<TextAsset>(aotDllName + ".dll.bytes").bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            }
            MainEntry.Log(MainEntry.LogCategory.Assets, "补充元数据Dll加载完毕==" + aotMetaAssemblyFiles.ToJson());
        }
    }
}
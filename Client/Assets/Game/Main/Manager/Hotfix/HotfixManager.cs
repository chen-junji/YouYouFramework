using HybridCLR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;

        public void Init()
        {
            //这里防止热更工程找不到AOT工程的类
            System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
            System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;

            MainEntry.ResourceManager.CheckVersionComplete = () =>
            {
                MainEntry.Download.BeginDownloadSingle(YFConstDefine.HotfixAssetBundlePath, onComplete: (string fileUrl) =>
                {
                    Debug.LogError(fileUrl);

                    //加载热更程序集
                    hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                    LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
                    System.Reflection.Assembly.Load(hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes").bytes);
#endif

                    MainEntry.Download.BeginDownloadSingle(YFConstDefine.GameEntryAssetBundlePath, onComplete: (string fileUrl) =>
                    {
                        Debug.LogError(fileUrl);

                        AssetBundle prefabAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                        Object.Instantiate(prefabAb.LoadAsset<GameObject>("gameentry.prefab"));
                    });

                });
            };

            MainEntry.ResourceManager.InitStreamingAssetsBundleInfo();

        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = hotfixAb.LoadAsset<TextAsset>(aotDllName + ".bytes").bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }
    }
}
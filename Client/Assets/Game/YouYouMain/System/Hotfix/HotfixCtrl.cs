using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYouMain;


/// <summary>
/// 代码热更新控制器
/// </summary>
public class HotfixCtrl
{
    public static HotfixCtrl Instance { get; private set; } = new();

    public AssetBundle hotfixAb;
    public static List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib",
            "System",
            "System.Core",
            "UniTask",
            "UnityEngine.CoreModule",
        };

    public HotfixCtrl()
    {
        //这里防止热更工程找不到AOT工程的类
        System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
        System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;
    }
    public void LoadHotifx()
    {
        VersionFileEntity versionFileEntity = VersionLocalModel.Instance.GetVersionFileEntity(YFConstDefine.HotfixAssetBundlePath);
        if (versionFileEntity != null)
        {
            //从可写区加载程序集
            hotfixAb = AssetBundle.LoadFromFile(Path.Combine(YFConstDefine.LocalAssetBundlePath, YFConstDefine.HotfixAssetBundlePath));
        }
        else
        {
            versionFileEntity = VersionStreamingModel.Instance.GetVersionFileEntity(YFConstDefine.HotfixAssetBundlePath);
            if (versionFileEntity != null)
            {
                //从只读区加载程序集
                hotfixAb = AssetBundle.LoadFromFile(Path.Combine(YFConstDefine.StreamingAssetBundlePath, YFConstDefine.HotfixAssetBundlePath));
            }
            else
            {
                MainEntry.LogError("Hotifx程序集不存在");
                return;
            }
        }

#if !UNITY_EDITOR
        LoadMetadataForAOTAssemblies();

        //加载热更Dll
        TextAsset hotfixAsset = hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes");
        System.Reflection.Assembly.Load(hotfixAsset.bytes);
        MainEntry.Log("Assembly-CSharp.dll加载完毕");
#endif
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private void LoadMetadataForAOTAssemblies()
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
        MainEntry.Log("补充元数据Dll加载完毕==" + aotMetaAssemblyFiles.ToJson());
    }
}
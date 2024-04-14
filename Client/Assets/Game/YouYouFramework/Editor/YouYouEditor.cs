using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class YouYouEditor : OdinMenuEditorWindow
{
    [MenuItem("YouYouTools/YouYouEditor")]
    private static void OpenYouYouEditor()
    {
        var window = GetWindow<YouYouEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 500);
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);

        //宏设置
        tree.AddAssetAtPath("MacroSettings", "Game/YouYouFramework/YouYouAssets/MacroSettings.asset");

        //参数设置
        tree.AddAssetAtPath("ParamsSettings", "Game/YouYouFramework/YouYouAssets/ParamsSettings.asset");

        //AssetBundle打包管理
        tree.AddAssetAtPath("AssetBundleSettings", "Game/YouYouFramework/YouYouAssets/AssetBundleSettings.asset");

        //类对象池
        tree.AddAssetAtPath("PoolAnalyze/ClassObjectPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_ClassObjectPool.asset");
        //AssetBundele池
        tree.AddAssetAtPath("PoolAnalyze/AssetBundlePool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetBundlePool.asset");
        //Asset池
        tree.AddAssetAtPath("PoolAnalyze/AssetPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetPool.asset");

        return tree;
    }
}

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

        //参数设置
        tree.AddAssetAtPath("ParamsSettings", "Game/YouYouMain/YouYouAssets/ParamsSettings.asset");

        //宏设置
        tree.AddAssetAtPath("MacroSettings", "Game/YouYouFramework/Editor/YouYouAssets/MacroSettings.asset");

        //AssetBundle打包管理
        tree.AddAssetAtPath("AssetBundleSettings", "Game/YouYouFramework/Editor/YouYouAssets/AssetBundleSettings.asset");

        //类对象池
        tree.AddAssetAtPath("ClassObjectPool", "Game/YouYouFramework/Editor/YouYouAssets/PoolAnalyze_ClassObjectPool.asset");
        //AssetBundele池
        tree.AddAssetAtPath("AssetBundlePool", "Game/YouYouFramework/Editor/YouYouAssets/PoolAnalyze_AssetBundlePool.asset");
        //Asset池
        tree.AddAssetAtPath("AssetPool", "Game/YouYouFramework/Editor/YouYouAssets/PoolAnalyze_AssetPool.asset");

        return tree;
    }
}

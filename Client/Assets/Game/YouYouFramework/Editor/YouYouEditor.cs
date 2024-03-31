using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

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

    #region AssetBundleOpenPersistentDataPath 打开persistentDataPath
    [MenuItem("YouYouTools/打开persistentDataPath")]
    public static void AssetBundleOpenPersistentDataPath()
    {
        string output = Application.persistentDataPath;
        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }
        output = output.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", output);
    }
    #endregion

    #region SetFBXAnimationMode 设置文件动画循环为true
    [MenuItem("YouYouTools/设置文件动画循环为true")]
    public static void SetFBXAnimationMode()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            string relatepath = AssetDatabase.GetAssetPath(objs[i]);

            if (relatepath.IsSuffix(".FBX", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath + ".meta";
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("loopTime: 0") != -1)
                    {
                        line = "      loopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }

            if (relatepath.IsSuffix(".Anim", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath;
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("m_LoopTime: 0") != -1)
                    {
                        line = "    m_LoopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }
        }
        AssetDatabase.Refresh();
    }
    #endregion


    #region GetAssetsPath 收集多个文件的路径到剪切板
    [MenuItem("YouYouTools/收集多个文件的路径到剪切板")]
    public static void GetAssetsPath()
    {
        Object[] objs = Selection.objects;
        string relatepath = string.Empty;
        for (int i = 0; i < objs.Length; i++)
        {
            relatepath += AssetDatabase.GetAssetPath(objs[i]);
            if (i < objs.Length - 1) relatepath += "\n";
        }
        Clipboard.Copy(relatepath);
        AssetDatabase.Refresh();
    }
    #endregion

}

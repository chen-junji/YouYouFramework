//===================================================
//备    注：替换代码注释
//===================================================
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UnityEditor;

/// <summary>
/// 
/// </summary>
public class ScriptCreatInit : UnityEditor.AssetModificationProcessor 
{
    private static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta","");
        if (path.EndsWith(".cs"))
        {
            string strContent = File.ReadAllText(path);
            strContent = strContent.Replace("#AuthorName#", "边涯").Replace("#CreateTime#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            File.WriteAllText(path, strContent);
            AssetDatabase.Refresh();
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using YouYou;


public class GameUtil
{
    /// <summary>
    /// 加载FBX嵌入的所有动画
    /// </summary>
    public static AnimationClip[] LoadInitRoleAnimationsByFBX(string path)
    {
#if EDITORLOAD && UNITY_EDITOR
        UnityEngine.Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (var item in objs)
        {
            if (item is AnimationClip) clips.Add(item as AnimationClip);
        }
        return clips.ToArray();
#else
        AssetInfoEntity m_CurrAssetEnity = GameEntry.Loader.GetAssetEntity(path);
        AssetBundle bundle = GameEntry.Loader.LoadAssetBundle(m_CurrAssetEnity.AssetBundleName);
        return bundle.LoadAllAssets<AnimationClip>();
#endif
    }

    /// <summary>
    /// 获取路径的最后名称
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetLastPathName(string path)
    {
        if (path.IndexOf('/') == -1)
        {
            return path;
        }
        return path.Substring(path.LastIndexOf('/') + 1);
    }

    /// <summary>
    /// 加载Prefab
    /// </summary>
    public static GameObject LoadPrefab(PrefabName prefabName)
    {
        Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName.ToString());
        return GameEntry.Loader.LoadMainAsset<GameObject>(sys_Prefab.AssetPath);
    }
    public static GameObject LoadPrefab(string prefabName)
    {
        Sys_PrefabEntity sys_Prefab = GameEntry.DataTable.Sys_PrefabDBModel.GetEntity(prefabName);
        return GameEntry.Loader.LoadMainAsset<GameObject>(sys_Prefab.AssetPath);
    }
}
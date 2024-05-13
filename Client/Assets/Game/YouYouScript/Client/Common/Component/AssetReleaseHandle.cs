using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


/// <summary>
/// Asset池, 对象被销毁时, 自动让引用计数-1
/// </summary>
public class AssetReleaseHandle : MonoBehaviour
{
    private List<AssetReferenceEntity> releaseList = new List<AssetReferenceEntity>();

    public static void Add(AssetReferenceEntity referenceEntity, GameObject target)
    {
        if (target == null)
        {
            GameObject SceneRoot = GameEntry.Pool.GameObjectPool.YouYouObjPool;
            target = SceneRoot;
            GameEntry.Log(LogCategory.Loader, string.Format("因为{0}没有可绑定的target， 所以绑定到了{1}上， 随当前场景销毁而减少引用计数", referenceEntity.AssetFullPath, SceneRoot));
        }

        if (target != null)
        {
            AssetReleaseHandle handle = target.GetOrCreatComponent<AssetReleaseHandle>();
            handle.releaseList.Add(referenceEntity);
            referenceEntity.ReferenceAdd();
        }
    }

    private void OnDestroy()
    {
        foreach (AssetReferenceEntity referenceEntity in releaseList)
        {
            referenceEntity.ReferenceRemove();
        }
    }
}

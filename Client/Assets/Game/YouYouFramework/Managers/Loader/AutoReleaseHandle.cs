using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class AutoReleaseHandle : MonoBehaviour
{
    private List<AssetReferenceEntity> releaseList = new List<AssetReferenceEntity>();

    public static void Add(AssetReferenceEntity referenceEntity, GameObject target)
    {
        if (target == null)
        {
            GameObject SceneRoot = null;//DOTO：这里是每个场景的对象池根节点，后续再补
            target = SceneRoot;
        }

        if (target != null)
        {
            AutoReleaseHandle handle = target.GetComponent<AutoReleaseHandle>();
            if (handle == null)
            {
                handle = target.AddComponent<AutoReleaseHandle>();
            }
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

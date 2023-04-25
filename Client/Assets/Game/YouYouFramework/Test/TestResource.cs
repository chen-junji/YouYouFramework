using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYou;

public class TestResource : MonoBehaviour
{
    async void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            GameObject testObj = GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<GameObject>("Role/RoleSources/cike/zhujiao_cike_animation.prefab");
            Debug.LogError(testObj);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            AssetBundle testObj = GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle("Role/RoleSources/cike/zhujiao_cike_animation.assetbundle");
            Debug.LogError(testObj);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            GameObject testObj = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<GameObject>("Role/RoleSources/cike/zhujiao_cike_animation.prefab");
            Debug.LogError(testObj);
        }
    }
}
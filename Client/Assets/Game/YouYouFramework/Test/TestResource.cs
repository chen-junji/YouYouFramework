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
            GameObject testObj = GameEntry.Resource.LoadMainAsset<GameObject>("Assets/Game/Download/Role/RoleSources/cike/zhujiao_cike_animation.prefab");
            Debug.LogError(testObj);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            AssetBundle testObj = GameEntry.Resource.LoadAssetBundle("Assets/Game/Download/Role/RoleSources/cike/zhujiao_cike_animation.assetbundle");
            Debug.LogError(testObj);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            GameObject testObj = await GameEntry.Resource.LoadMainAssetAsync<GameObject>("Assets/Game/Download/Role/RoleSources/cike/zhujiao_cike_animation.prefab");
            Debug.LogError(testObj);
        }
    }
}
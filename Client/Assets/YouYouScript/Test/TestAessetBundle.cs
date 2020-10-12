//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YouYou;

public class TestAessetBundle : MonoBehaviour
{

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            //GameEntry.Pool.GameObjectSpawn
            //GameEntry.Pool.GameObjectSpawn(1, null);

			string path = @"G:\Project\YouYou_MMORPG\YFYouYou\Client\AssetBundles\1.0.0\Windows\download\role\rolesources\cike\zhujiao_cike_animation.assetbundle";
			AssetBundle bundle = AssetBundle.LoadFromFile(path);
			Debug.LogError(bundle.GetAllAssetNames().ToJson());

			Object[] objs = bundle.LoadAllAssets<AnimationClip>();
			for (int i = 0; i < objs.Length; i++)
			{
				Debug.LogError(objs[i]);
			}
		}
    }
}
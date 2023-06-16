using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.Networking;

namespace Main
{
    /// <summary>
    /// StreamingAssets管理器
    /// </summary>
    public class StreamingAssetsManager
    {
        public void LoadAssetBundleAction(string fileUrl, Action<byte[]> onComplete)
        {
            IEnumerator ReadStreamingAsset(string url, Action<byte[]> onComplete)
            {
                var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, url));
                using (UnityWebRequest request = UnityWebRequest.Get(uri.AbsoluteUri))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        onComplete?.Invoke(request.downloadHandler.data);
                    }
                    else
                    {
                        onComplete?.Invoke(null);
                    }
                }
            }
            MainEntry.Instance.StartCoroutine(ReadStreamingAsset(fileUrl + "AssetBundle", onComplete));
        }
    }
}
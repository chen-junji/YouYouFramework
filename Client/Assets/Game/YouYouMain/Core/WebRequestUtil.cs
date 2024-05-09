using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Networking;
using YouYouMain;

public class WebRequestUtil
{
    //为什么不用UniTask await UnityWebRequest.Get(uri).SendWebRequest()?
    //因为它加载失败时无法触发return, 所以现在用了IEnumerator
    private static IEnumerator LoadBytes(string assetFullPath, Action<byte[]> onComplete)
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(assetFullPath);
        yield return unityWebRequest.SendWebRequest();
        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            onComplete?.Invoke(unityWebRequest.downloadHandler.data);
        }
        else
        {
            onComplete?.Invoke(null);
            //Debug.LogError(unityWebRequest.error);
        }
    }
    public static async UniTask<byte[]> LoadCDNBytesAsync(string assetFullPath)
    {
        //为什么不用await UnityWebRequest.Get(uri).SendWebRequest()?
        //因为它加载失败时无法触发return
        var task = new UniTaskCompletionSource<byte[]>();
        MainEntry.Instance.StartCoroutine(LoadBytes(assetFullPath, (byte[] bytes) =>
        {
            task.TrySetResult(bytes);
        }));
        return await task.Task;
    }
    public static async UniTask<byte[]> LoadStreamingBytesAsync(string assetFullPath)
    {
        string uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "AssetBundles", assetFullPath)).AbsoluteUri;
        var task = new UniTaskCompletionSource<byte[]>();
        MainEntry.Instance.StartCoroutine(LoadBytes(uri, (byte[] bytes) =>
        {
            task.TrySetResult(bytes);
        }));
        return await task.Task;
    }

    public static async UniTask<AssetBundle> LoadStreamingAssetBundleAsync(string assetFullPath)
    {
        //为什么不用UniTask await UnityWebRequest.Get(uri).SendWebRequest()?
        //因为它加载失败时无法触发return, 所以现在用了IEnumerator
        IEnumerator LoadAssetBundle(string assetFullPath, Action<AssetBundle> onComplete)
        {
            string uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "AssetBundles", assetFullPath)).AbsoluteUri;
            UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            yield return unityWebRequest.SendWebRequest();
            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest);
                onComplete?.Invoke(bundle);
            }
            else
            {
                onComplete?.Invoke(null);
                //Debug.LogError(unityWebRequest.error);
            }
        }
        var task = new UniTaskCompletionSource<AssetBundle>();
        MainEntry.Instance.StartCoroutine(LoadAssetBundle(assetFullPath, (AssetBundle assetBundle) =>
        {
            task.TrySetResult(assetBundle);
        }));
        return await task.Task;
    }
    public static AssetBundle LoadStreamingAssetBundle(string assetFullPath)
    {
        string uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, "AssetBundles", assetFullPath)).AbsoluteUri;
        UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(unityWebRequest);
        return bundle;
    }
}

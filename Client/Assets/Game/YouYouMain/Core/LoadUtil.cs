using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using YouYouMain;

public class LoadUtil 
{
    public static Dictionary<string, VersionFileEntity> LoadVersionFile(byte[] buffer, ref string version)
    {
        buffer = ZlibHelper.DeCompressBytes(buffer);

        Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();

        MMO_MemoryStream ms = new MMO_MemoryStream(buffer);

        int len = ms.ReadInt();

        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                version = ms.ReadUTF8String().Trim();
            }
            else
            {
                VersionFileEntity entity = new VersionFileEntity();
                entity.AssetBundleName = ms.ReadUTF8String();
                entity.MD5 = ms.ReadUTF8String();
                entity.Size = ms.ReadULong();
                entity.IsFirstData = ms.ReadByte() == 1;
                entity.IsEncrypt = ms.ReadByte() == 1;

                dic[entity.AssetBundleName] = entity;
            }
        }
        return dic;
    }

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
        string uri = new System.Uri(Path.Combine(YFConstDefine.StreamingAssetBundlePath, assetFullPath)).AbsoluteUri;
        var task = new UniTaskCompletionSource<byte[]>();
        MainEntry.Instance.StartCoroutine(LoadBytes(uri, (byte[] bytes) =>
        {
            task.TrySetResult(bytes);
        }));
        return await task.Task;
    }

}

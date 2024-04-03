using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YouYouMain;

public class VersionFileCtrl
{
    public static VersionFileCtrl Instance { get; private set; } = new VersionFileCtrl();


    /// <summary>
    /// 去资源站点请求CDN的版本文件信息
    /// </summary>
    public void SendCDNVersionFile(Action onInitComplete)
    {
        StringBuilder sbr = StringHelper.PoolNew();
        string url = sbr.AppendFormatNoGC("{0}{1}", SystemModel.Instance.CurrChannelConfig.RealSourceUrl, YFConstDefine.VersionFileName).ToString();
        StringHelper.PoolDel(ref sbr);

        IEnumerator UnityWebRequestGet(string url, Action<UnityWebRequest> onComplete)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                onComplete?.Invoke(request);
            }
        }
        MainEntry.Instance.StartCoroutine(UnityWebRequestGet(url, (request) =>
        {
            //CDN版本文件请求成功
            if (request.result == UnityWebRequest.Result.Success)
            {
                //加载CDN版本文件信息
                LoadCDNVersionFile(request.downloadHandler.data);

                //加载可写区版本文件信息
                LoadLocalVersionFile();

                onInitComplete?.Invoke();
            }
            else
            {
                MainEntry.Log("初始化CDN资源包信息失败，url==" + url);
            }
        }));
    }
    private void LoadCDNVersionFile(byte[] buffer)
    {
        buffer = ZlibHelper.DeCompressBytes(buffer);

        Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();

        MMO_MemoryStream ms = new MMO_MemoryStream(buffer);

        int len = ms.ReadInt();

        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                VersionCDNModel.Instance.CDNVersion = ms.ReadUTF8String().Trim();
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
        VersionCDNModel.Instance.VersionDic = dic;
        MainEntry.Log("OnInitCDNVersionFile");
    }
    private void LoadLocalVersionFile()
    {
        //判断可写区版本文件是否存在
        if (VersionLocalModel.Instance.GetVersionFileExists())
        {
            string json = IOUtil.GetFileText(VersionLocalModel.Instance.LocalVersionFilePath);
            VersionLocalModel.Instance.LocalAssetsVersionDic = json.ToObject<Dictionary<string, VersionFileEntity>>();
            VersionLocalModel.Instance.LocalAssetsVersion = PlayerPrefs.GetString(YFConstDefine.AssetVersion);
            MainEntry.Log("OnInitLocalVersionFile");
        }
    }
}

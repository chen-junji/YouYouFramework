using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    /// <summary>
    /// 可写区资源管理器
    /// </summary>
    public class VersionFileManager
    {
        /// <summary>
        /// 根据字节数组获取资源包版本信息
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public Dictionary<string, VersionFileEntity> GetAssetBundleVersionList(byte[] buffer, ref string version)
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

        #region CDN
        /// <summary>
        /// CDN资源版本号
        /// </summary>
        private string m_CDNVersion;

        /// <summary>
        /// CDN资源版本号
        /// </summary>
        public string CDNVersion { get { return m_CDNVersion; } }

        /// <summary>
        /// CDN资源包信息
        /// </summary>
        public Dictionary<string, VersionFileEntity> m_CDNVersionDic = new Dictionary<string, VersionFileEntity>();

        /// <summary>
        /// 初始化CDN的版本文件信息
        /// </summary>
        public void InitCDNVersionFile(Action onInitComplete)
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
                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_CDNVersionDic = GetAssetBundleVersionList(request.downloadHandler.data, ref m_CDNVersion);
                    MainEntry.Log(MainEntry.LogCategory.Assets, "OnInitCDNVersionFile");
                    if (GetVersionFileExists())
                    {
                        //可写区版本文件存在，加载版本文件信息
                        m_LocalAssetsVersionDic = GetAssetBundleVersionList(ref m_LocalAssetsVersion);
                        MainEntry.Log(MainEntry.LogCategory.Assets, "OnInitLocalVersionFile");
                    }
                    onInitComplete?.Invoke();
                }
                else
                {
                    Main.MainEntry.Log(MainEntry.LogCategory.Assets, "初始化CDN资源包信息失败，url==" + url);
                }
            }));
        }

        /// <summary>
        /// 获取CDN上的资源包的版本信息(这个方法一定要能返回信息)
        /// </summary>
        /// <param name="assetbundlePath"></param>
        /// <returns></returns>
        public VersionFileEntity GetVersionFileEntity(string assetbundlePath)
        {
            VersionFileEntity entity = null;
            m_CDNVersionDic.TryGetValue(assetbundlePath, out entity);
            return entity;
        }

        #endregion


        #region 可写区
        /// <summary>
        /// 可写区 版本文件路径
        /// </summary>
        public string LocalVersionFilePath
        {
            get
            {
                return string.Format("{0}/{1}", Application.persistentDataPath, YFConstDefine.VersionFileName);
            }
        }

        /// <summary>
        /// 可写区资源版本号
        /// </summary>
        public string LocalAssetsVersion { get { return m_LocalAssetsVersion; } }
        private string m_LocalAssetsVersion;

        /// <summary>
        /// 可写区资源包信息
        /// </summary>
        public Dictionary<string, VersionFileEntity> m_LocalAssetsVersionDic = new Dictionary<string, VersionFileEntity>();

        /// <summary>
        /// 保存可写区版本信息
        /// </summary>
        /// <param name="entity"></param>
        public void SaveVersion(VersionFileEntity entity)
        {
            m_LocalAssetsVersionDic[entity.AssetBundleName] = entity;

            //保存版本文件
            string json = m_LocalAssetsVersionDic.ToJson();
            IOUtil.CreateTextFile(LocalVersionFilePath, json);
        }

        /// <summary>
        /// 保存可写区资源版本号
        /// </summary>
        public void SetAssetVersion(string version)
        {
            m_LocalAssetsVersion = version;
            PlayerPrefs.SetString(YFConstDefine.AssetVersion, version);
        }

        /// <summary>
        /// 获取可写区版本文件是否存在
        /// </summary>
        /// <returns></returns>
        public bool GetVersionFileExists()
        {
            return File.Exists(LocalVersionFilePath);
        }

        /// <summary>
        /// 加载可写区资源包信息
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public Dictionary<string, VersionFileEntity> GetAssetBundleVersionList(ref string version)
        {
            version = PlayerPrefs.GetString(YFConstDefine.AssetVersion);
            string json = IOUtil.GetFileText(LocalVersionFilePath);
            return json.ToObject<Dictionary<string, VersionFileEntity>>();
        }

        #endregion

    }
}

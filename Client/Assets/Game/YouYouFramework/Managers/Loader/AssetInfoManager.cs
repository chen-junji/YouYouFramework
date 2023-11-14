using Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YouYou
{
    public class AssetInfoManager
    {
        /// <summary>
        /// 资源信息字典
        /// </summary>
        private Dictionary<string, AssetInfoEntity> m_AssetInfoDic;

        private Action m_InitAssetInfoComplete;

        public AssetInfoManager()
        {
            m_AssetInfoDic = new Dictionary<string, AssetInfoEntity>();
        }

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        internal async void InitAssetInfo(Action initAssetInfoComplete)
        {
            m_InitAssetInfoComplete = initAssetInfoComplete;

            byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, YFConstDefine.AssetInfoName));
            if (buffer == null)
            {
                //如果可写区没有,从CDN读取
                string url = string.Format("{0}{1}", SystemModel.Instance.CurrChannelConfig.RealSourceUrl, YFConstDefine.AssetInfoName);
                HttpCallBackArgs args = await GameEntry.Http.GetArgsAsync(url, false);
                if (!args.HasError)
                {
                    GameEntry.Log(LogCategory.Loader, "从CDN初始化资源信息");
                    InitAssetInfo(args.Data);
                }
            }
            else
            {
                GameEntry.Log(LogCategory.Loader, "从可写区初始化资源信息");
                InitAssetInfo(buffer);
            }
        }

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        private void InitAssetInfo(byte[] buffer)
        {
            buffer = ZlibHelper.DeCompressBytes(buffer);//解压

            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
            int len = ms.ReadInt();
            int depLen = 0;
            for (int i = 0; i < len; i++)
            {
                AssetInfoEntity entity = new AssetInfoEntity();
                entity.AssetFullName = ms.ReadUTF8String();
                entity.AssetBundleName = ms.ReadUTF8String();

                //GameEntry.Log("entity.AssetBundleName=" + entity.AssetBundleName);
                //GameEntry.Log("entity.AssetFullName=" + entity.AssetFullName);

                depLen = ms.ReadInt();
                if (depLen > 0)
                {
                    entity.DependsAssetBundleList = new List<string>(depLen);
                    for (int j = 0; j < depLen; j++)
                    {
                        entity.DependsAssetBundleList.Add(ms.ReadUTF8String());
                    }
                }

                m_AssetInfoDic[entity.AssetFullName] = entity;
            }

            m_InitAssetInfoComplete?.Invoke();
        }

        /// <summary>
        /// 根据资源路径获取资源信息
        /// </summary>
        internal AssetInfoEntity GetAssetEntity(string assetFullName)
        {
            AssetInfoEntity entity = null;
            if (m_AssetInfoDic.TryGetValue(assetFullName, out entity))
            {
                return entity;
            }
            GameEntry.LogError(LogCategory.Loader, "资源不存在, assetFullName=>{0}", assetFullName);
            return null;
        }

    }
}

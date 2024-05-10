using Cysharp.Threading.Tasks;
using YouYouMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace YouYouFramework
{
    public class AssetInfoModel
    {
        /// <summary>
        /// 资源信息字典
        /// </summary>
        private Dictionary<string, AssetInfoEntity> m_AssetInfoDic;

        public AssetInfoModel()
        {
            m_AssetInfoDic = new Dictionary<string, AssetInfoEntity>();
        }

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        internal async UniTask<bool> InitAssetInfo()
        {
            //从可写区获取
            byte[] buffer = IOUtil.GetFileBuffer(YFConstDefine.LocalAssetInfoPath);
            if (buffer != null)
            {
                GameEntry.Log(LogCategory.Loader, "从可写区初始化资源依赖信息");
                InitAssetInfo(buffer);
                return true;
            }

            //如果可写区没有 那么就从只读区获取
            byte[] buff = await WebRequestUtil.LoadStreamingBytesAsync(YFConstDefine.AssetInfoName);
            if (buff != null)
            {
                GameEntry.Log(LogCategory.Loader, "从只读区初始化资源依赖信息");
                InitAssetInfo(buff);
                return true;
            }

            //如果只读区也没有,从CDN获取
            string url = Path.Combine(ChannelModel.Instance.CurrChannelConfig.RealSourceUrl, YFConstDefine.AssetInfoName);
            HttpCallBackArgs args = await GameEntry.Http.GetArgsAsync(url, false);
            if (!args.HasError)
            {
                GameEntry.Log(LogCategory.Loader, "从CDN初始化资源依赖信息");
                InitAssetInfo(args.Data);
                return true;
            }
            return false;
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
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.AssetBundleFullPath = ms.ReadUTF8String();

                //GameEntry.Log("entity.AssetBundleName=" + entity.AssetBundleName);
                //GameEntry.Log("entity.AssetFullName=" + entity.AssetFullName);

                entity.DependsAssetBundleList = new List<string>();
                depLen = ms.ReadInt();
                if (depLen > 0)
                {
                    for (int j = 0; j < depLen; j++)
                    {
                        entity.DependsAssetBundleList.Add(ms.ReadUTF8String());
                    }
                }

                m_AssetInfoDic[entity.AssetFullPath] = entity;
            }
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
            GameEntry.LogError(LogCategory.Loader, string.Format("资源不存在, assetFullName=>{0}", assetFullName));
            return null;
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace YouYouFramework
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    public class AssetLoaderRoutine
    {
        /// <summary>
        /// 资源加载请求
        /// </summary>
        public AssetBundleRequest CurrAssetBundleRequest { get; private set; }

        public string CurrAssetName { get; private set; }

        /// <summary>
        /// 资源请求更新
        /// </summary>
        public Action<float> OnAssetUpdate;

        /// <summary>
        /// 加载资源完毕
        /// </summary>
        public Action<Object> OnLoadAssetComplete;


        internal void LoadAssetAsync(string assetName, AssetBundle assetBundle)
        {
            CurrAssetName = assetName;
            CurrAssetBundleRequest = assetBundle.LoadAssetAsync(assetName);
        }
        internal Object LoadAsset(string assetName, AssetBundle assetBundle)
        {
            return assetBundle.LoadAsset(assetName);
        }

        public static AssetLoaderRoutine Create()
        {
            AssetLoaderRoutine assetLoaderRoutine = GameEntry.Pool.ClassObjectPool.Dequeue<AssetLoaderRoutine>();
            return assetLoaderRoutine;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            CurrAssetBundleRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            UpdateAssetBundleRequest();
        }

        /// <summary>
        /// 更新 资源加载 请求
        /// </summary>
        private void UpdateAssetBundleRequest()
        {
            if (CurrAssetBundleRequest != null)
            {
                if (CurrAssetBundleRequest.isDone)
                {
                    Object obj = CurrAssetBundleRequest.asset;
                    if (obj != null)
                    {
                        //GameEntry.Log(LogCategory.Loader, "资源=>{0} 加载完毕", m_CurrAssetName);
                    }
                    else
                    {
                        GameEntry.LogError(LogCategory.Loader, string.Format("资源=>{0} 加载失败", CurrAssetName));
                    }
                    Reset();//一定要早点Reset
                    GameEntry.Pool.ClassObjectPool.Enqueue(this);
                    OnLoadAssetComplete?.Invoke(obj);
                }
                else
                {
                    //加载进度
                    OnAssetUpdate?.Invoke(CurrAssetBundleRequest.progress);
                }
            }
        }
    }
}
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

        public string AssetFullPath { get; private set; }

        /// <summary>
        /// 资源请求更新
        /// </summary>
        public Action<float> OnAssetUpdate;

        /// <summary>
        /// 加载资源完毕
        /// </summary>
        public Action<Object> OnLoadAssetComplete;


        internal void LoadAssetAsync(string assetFullPath, AssetBundle assetBundle)
        {
            AssetFullPath = assetFullPath;
            CurrAssetBundleRequest = assetBundle.LoadAssetAsync(assetFullPath);
        }

        public static AssetLoaderRoutine Create()
        {
            AssetLoaderRoutine assetLoaderRoutine = GameEntry.Pool.ClassObjectPool.Dequeue<AssetLoaderRoutine>();
            return assetLoaderRoutine;
        }
        internal void OnUpdate()
        {
            if (CurrAssetBundleRequest != null)
            {
                if (CurrAssetBundleRequest.isDone)
                {
                    Object obj = CurrAssetBundleRequest.asset;
                    if (obj != null)
                    {
                        //GameEntry.Log(LogCategory.Loader, $"资源=>{m_CurrAssetName} 加载完毕");
                    }
                    else
                    {
                        GameEntry.LogError(LogCategory.Loader, string.Format("资源=>{0} 加载失败", AssetFullPath));
                    }
                    OnLoadAssetComplete?.Invoke(obj);
                    Reset();//一定要早点Reset
                }
                else
                {
                    //加载进度
                    OnAssetUpdate?.Invoke(CurrAssetBundleRequest.progress);
                }
            }
        }
        public void Reset()
        {
            CurrAssetBundleRequest = null;
            AssetFullPath = null;
            OnAssetUpdate = null;
            OnLoadAssetComplete = null;
            GameEntry.Pool.ClassObjectPool.Enqueue(this);
        }

    }
}
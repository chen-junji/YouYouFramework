using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 资源包加载器
    /// </summary>
    public class AssetBundleLoaderRoutine
    {
        /// <summary>
        /// 当前的资源包信息
        /// </summary>
        public AssetBundleInfoEntity CurrAssetBundleInfo { get; private set; }

        /// <summary>
        /// 资源包创建请求
        /// </summary>
        public AssetBundleCreateRequest CurrAssetBundleCreateRequest { get; private set; }

        /// <summary>
        /// 资源包创建请求更新
        /// </summary>
        public Action<float> OnAssetBundleCreateUpdate;

        /// <summary>
        /// 加载资源包完毕
        /// </summary>
        public Action<AssetBundle> OnLoadAssetBundleComplete;

        #region LoadAssetBundle 加载资源包
        public void LoadAssetBundleAsync(string assetBundlePath)
        {
            void LoadAssetBundleAsync(byte[] buffer)
            {
                if (CurrAssetBundleInfo.IsEncrypt)
                {
                    //如果资源包是加密的,则解密
                    buffer = SecurityUtil.Xor(buffer);
                }

                CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
            }


            CurrAssetBundleInfo = MainEntry.ResourceManager.GetAssetBundleInfo(assetBundlePath);

            //检查文件在可写区是否存在
            bool isExistsInLocal = MainEntry.ResourceManager.LocalAssetsManager.CheckFileExists(assetBundlePath);

            if (isExistsInLocal && !CurrAssetBundleInfo.IsEncrypt)
            {
                //可写区加载, 不用解密
                CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
            }
            else
            {
                //可写区加载, 需要解密
                byte[] buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(assetBundlePath);
                if (buffer != null)
                {
                    LoadAssetBundleAsync(buffer);
                    return;
                }

                //如果可写区没有 那么就从只读区获取
                MainEntry.ResourceManager.StreamingAssetsManager.ReadAssetBundleAsync(assetBundlePath, (byte[] buff) =>
                {
                    if (buff != null)
                    {
                        //从只读区加载资源包
                        LoadAssetBundleAsync(buff);
                        return;
                    }

                    //如果只读区也没有,从CDN下载
                    MainEntry.Download.BeginDownloadSingle(assetBundlePath, (url, currSize, progress) =>
                    {
                        //YouYou.GameEntry.LogError(progress);
                        OnAssetBundleCreateUpdate?.Invoke(progress);
                    }, (string fileUrl) =>
                    {
                        buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(fileUrl);
                        LoadAssetBundleAsync(buffer);
                    });
                });
            }

        }
        public AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            AssetBundle LoadAssetBundle(byte[] buffer)
            {
                if (CurrAssetBundleInfo.IsEncrypt)
                {
                    //如果资源包是加密的,则解密
                    buffer = SecurityUtil.Xor(buffer);
                }

                return AssetBundle.LoadFromMemory(buffer);
            }


            CurrAssetBundleInfo = MainEntry.ResourceManager.GetAssetBundleInfo(assetBundlePath);

            //检查文件在可写区是否存在
            bool isExistsInLocal = MainEntry.ResourceManager.LocalAssetsManager.CheckFileExists(assetBundlePath);

            if (isExistsInLocal && !CurrAssetBundleInfo.IsEncrypt)
            {
                //可写区加载, 不用解密
                return AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
            }
            else
            {
                //可写区加载, 需要解密
                byte[] buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(assetBundlePath);
                if (buffer != null)
                {
                    return LoadAssetBundle(buffer);
                }

                //只读区加载(目前不支持加密资源)
                AssetBundle assetBundle = MainEntry.ResourceManager.StreamingAssetsManager.ReadAssetBundle(assetBundlePath);
                if (assetBundle != null)
                {
                    return assetBundle;
                }

                GameEntry.LogError(LogCategory.Resource, "本地没有该资源, 或许要去服务端下载==" + assetBundlePath);
                return null;
            }

        }

        #endregion

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            CurrAssetBundleCreateRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            UpdateAssetBundleCreateRequest();
        }

        #region UpdateAssetBundleCreateRequest 更新资源包请求
        /// <summary>
        /// 更新资源包请求
        /// </summary>
        private void UpdateAssetBundleCreateRequest()
        {
            if (CurrAssetBundleCreateRequest == null) return;
            if (CurrAssetBundleCreateRequest.isDone)
            {
                AssetBundle assetBundle = CurrAssetBundleCreateRequest.assetBundle;
                if (assetBundle != null)
                {
                    //GameEntry.Log(LogCategory.Resource, "资源包=>{0} 加载完毕", m_CurrAssetBundleInfo.AssetBundleName);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Resource, "资源包=>{0} 加载失败", CurrAssetBundleInfo.AssetBundleName);
                }
                OnLoadAssetBundleComplete?.Invoke(assetBundle);
                Reset();//一定要早点Reset
            }
            else
            {
                //加载进度
                //OnAssetBundleCreateUpdate?.Invoke(m_CurrAssetBundleCreateRequest.progress);
            }
        }
        #endregion
    }
}
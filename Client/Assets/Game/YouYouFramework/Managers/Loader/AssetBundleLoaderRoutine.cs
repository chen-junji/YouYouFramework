using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.Serialization;


namespace YouYouFramework
{
    /// <summary>
    /// 资源包加载器
    /// </summary>
    public class AssetBundleLoaderRoutine
    {
        /// <summary>
        /// 当前的资源包信息
        /// </summary>
        public VersionFileEntity CurrVersionFile { get; private set; }

        /// <summary>
        /// 资源包创建请求
        /// </summary>
        public AssetBundleCreateRequest CurrAssetBundleCreateRequest { get; private set; }

        /// <summary>
        /// 加载资源包完毕
        /// </summary>
        public Action<AssetBundle> OnLoadAssetBundleComplete;

        /// <summary>
        /// 资源包创建请求更新
        /// </summary>
        public Action<float> OnAssetBundleCreateUpdate;

        /// <summary>
        /// 资源包下载请求更新（边玩边下载才会触发）
        /// </summary>
        public Action<float> OnAssetBundleDownloadUpdate;

        #region LoadAssetBundle 加载资源包
        public async void LoadAssetBundleAsync(string assetBundlePath)
        {
            //检查文件在可写区是否存在
            CurrVersionFile = VersionLocalModel.Instance.GetVersionFileEntity(assetBundlePath);
            if (CurrVersionFile != null)
            {
                if (CurrVersionFile.IsEncrypt)
                {
                    //需要解密
                    byte[] buffer = IOUtil.GetFileBuffer(Path.Combine(MainConstDefine.LocalAssetBundlePath, assetBundlePath));
                    if (buffer != null)
                    {
                        buffer = SecurityUtil.Xor(buffer);
                        CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
                    }
                }
                else
                {
                    //不用解密
                    CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(MainConstDefine.LocalAssetBundlePath, assetBundlePath));
                }
                return;
            }

            //可写区没有 检查文件在只读区是否存在
            CurrVersionFile = VersionStreamingModel.Instance.GetVersionFileEntity(assetBundlePath);
            if (CurrVersionFile != null)
            {
                if (CurrVersionFile.IsEncrypt)
                {
                    //需要解密
                    byte[] buff = await LoadUtil.LoadStreamingBytesAsync(assetBundlePath);
                    if (buff != null)
                    {
                        buff = SecurityUtil.Xor(buff);
                        CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buff);
                    }
                }
                else
                {
                    //不用解密
                    CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(MainConstDefine.StreamingAssetBundlePath, assetBundlePath));
                }
                return;
            }


            //如果只读区也没有,从CDN下载(边玩边下载)
            MainEntry.Download.BeginDownloadSingle(assetBundlePath, (url, currSize, progress) =>
            {
                //YouYou.GameEntry.LogError(progress);
                OnAssetBundleCreateUpdate?.Invoke(progress);
            }, (string fileUrl) =>
            {
                //下载完毕，从可写区加载
                LoadAssetBundleAsync(assetBundlePath);
            });

        }
        public static AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            AssetBundle assetBundle = null;

            //检查文件在可写区是否存在
            VersionFileEntity versionFile = VersionLocalModel.Instance.GetVersionFileEntity(assetBundlePath);
            if (versionFile != null)
            {
                if (versionFile.IsEncrypt)
                {
                    //需要解密
                    byte[] buffer = IOUtil.GetFileBuffer(Path.Combine(MainConstDefine.LocalAssetBundlePath, assetBundlePath));
                    if (buffer != null)
                    {
                        buffer = SecurityUtil.Xor(buffer);
                        assetBundle = AssetBundle.LoadFromMemory(buffer);
                    }
                }
                else
                {
                    //不用解密
                    assetBundle = AssetBundle.LoadFromFile(Path.Combine(MainConstDefine.LocalAssetBundlePath, assetBundlePath));
                }
                return assetBundle;
            }

            //如果可写区没有 检查文件在只读区是否存在
            versionFile = VersionStreamingModel.Instance.GetVersionFileEntity(assetBundlePath);
            if (versionFile != null)
            {
                if (versionFile.IsEncrypt)
                {
                    //只读区加载, 需要解密
                    GameEntry.LogError(LogCategory.Loader, "只读区 同步加载 暂时不支持解密==" + assetBundlePath);
                }
                else
                {
                    //只读区加载, 不用解密
                    assetBundle = AssetBundle.LoadFromFile(Path.Combine(MainConstDefine.StreamingAssetBundlePath, assetBundlePath));
                }
                return assetBundle;
            }

            //注意: 同步加载不支持边玩边下载
            return assetBundle;
        }

        #endregion

        public static AssetBundleLoaderRoutine Create()
        {
            AssetBundleLoaderRoutine assetBundleLoaderRoutine = GameEntry.Pool.ClassObjectPool.Dequeue<AssetBundleLoaderRoutine>();
            return assetBundleLoaderRoutine;
        }

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
                    //GameEntry.Log(LogCategory.Loader, string.Format("资源包=>{0} 加载完毕", m_CurrAssetBundleInfo.AssetBundleName));
                }
                else
                {
                    GameEntry.LogError(LogCategory.Loader, string.Format("资源包=>{0} 加载失败", CurrVersionFile.AssetBundleName));
                }
                Reset();//一定要早点Reset
                GameEntry.Pool.ClassObjectPool.Enqueue(this);
                OnLoadAssetBundleComplete?.Invoke(assetBundle);
            }
            else
            {
                //加载进度
                OnAssetBundleCreateUpdate?.Invoke(CurrAssetBundleCreateRequest.progress);
            }
        }
        #endregion
    }
}
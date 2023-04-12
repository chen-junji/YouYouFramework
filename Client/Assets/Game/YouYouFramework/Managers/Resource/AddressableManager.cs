using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 可寻址资源管理器
    /// </summary>
    public class AddressableManager 
    {
        /// <summary>
        /// 资源管理器
        /// </summary>
        public ResourceManager ResourceManager { get; private set; }

        /// <summary>
        /// 资源加载管理器
        /// </summary>
        public ResourceLoaderManager ResourceLoaderManager { get; private set; }

        /// <summary>
        /// 本地文件路径
        /// </summary>
        public string LocalFilePath { get; private set; }

        internal AddressableManager()
        {
            ResourceManager = new ResourceManager();
            ResourceLoaderManager = new ResourceLoaderManager();
        }
        internal void Init()
        {
#if EDITORLOAD
            LocalFilePath = Application.dataPath;
#else
            LocalFilePath = Application.persistentDataPath;
#endif

            ResourceManager.Init();
            ResourceLoaderManager.Init();

            Application.backgroundLoadingPriority = ThreadPriority.High;
        }
        internal void OnUpdate()
        {
            ResourceLoaderManager.OnUpdate();
        }

        /// <summary>
        /// 初始化只读区资源包信息
        /// </summary>
        public void InitStreamingAssetsBundleInfo()
        {
            ResourceManager.InitStreamingAssetsBundleInfo();
        }


        /// <summary>
        /// 获取路径的最后名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetLastPathName(string path)
        {
            if (path.IndexOf('/') == -1)
            {
                return path;
            }
            return path.Substring(path.LastIndexOf('/') + 1);
        }
        /// <summary>
        /// 获取场景的资源包路径
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public string GetSceneAssetBundlePath(string sceneName)
        {
            //string.Format("download/scenes/{0}.assetbundle", sceneName.ToLower());
            return string.Format("Scenes/{0}.unity", sceneName.ToLower());
        }
    }
}
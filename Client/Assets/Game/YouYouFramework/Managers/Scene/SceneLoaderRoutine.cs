using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YouYou;
using System;
using Object = UnityEngine.Object;
using Main;

namespace YouYou
{
    /// <summary>
    /// 场景加载和卸载器
    /// </summary>
    public class SceneLoaderRoutine
    {
        private AsyncOperation m_CurrAsync = null;

        private string SceneFullPath;

        /// <summary>
        /// 进度更新
        /// </summary>
        private Action<string, float> OnProgressUpdate;

        /// <summary>
        /// 加载场景
        /// </summary>
        public async void LoadScene(string sceneFullPath, Action<string, float> onProgressUpdate)
        {
            SceneFullPath = sceneFullPath;

            OnProgressUpdate = onProgressUpdate;

            if (MainEntry.IsAssetBundleMode)
            {
                //加载场景的资源包
                await GameEntry.Loader.LoadAssetBundleMainAndDependAsync(sceneFullPath);

                //场景只需要给AssetBundle做引用计数， 不需要给Asset做引用计数
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(sceneFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceAdd();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceAdd();
                }
            }
            m_CurrAsync = SceneManager.LoadSceneAsync(sceneFullPath, LoadSceneMode.Additive);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public void UnLoadScene(string sceneFullPath)
        {
            if (MainEntry.IsAssetBundleMode)
            {
                //场景只需要给AssetBundle做引用计数， 不需要给Asset做引用计数
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(sceneFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceRemove();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceRemove();
                }
            }
            m_CurrAsync = SceneManager.UnloadSceneAsync(sceneFullPath);
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            if (m_CurrAsync == null) return;
            if (!m_CurrAsync.isDone)
            {
                OnProgressUpdate?.Invoke(SceneFullPath, m_CurrAsync.progress);
            }
        }
    }
}
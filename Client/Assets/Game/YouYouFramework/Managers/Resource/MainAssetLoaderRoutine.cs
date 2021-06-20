using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 主资源加载器
    /// </summary>
    public class MainAssetLoaderRoutine
    {
        /// <summary>
        /// 当前的资源信息实体
        /// </summary>
        private AssetEntity m_CurrAssetEntity;

        /// <summary>
        /// 当前的资源实体
        /// </summary>
        private ResourceEntity m_CurrResourceEntity;

        /// <summary>
        /// 主资源加载进度刷新
        /// </summary>
        private Action<float> m_OnUpdate;
        /// <summary>
        /// 主资源加载完毕
        /// </summary>
        private Action<ResourceEntity> m_OnComplete;

        /// <summary>
        /// 主资源包
        /// </summary>
        private AssetBundle m_MainAssetBundle;

        /// <summary>
        /// 依赖资源包名字哈希
        /// </summary>
        private HashSet<string> m_DependsAssetBundleNames = new HashSet<string>();

        /// <summary>
        /// 是否递增引用计数
        /// </summary>
        private bool m_IsAddReferenceCount;

        /// <summary>
        /// 加载主资源(包括依赖)
        /// </summary>
        /// <param name="assetFullName"></param>
        /// <param name="isParallel">True并行加载, Flase递归加载</param>
        internal void Load(string assetFullName, Action<ResourceEntity> onComplete, Action<float> onUpdate, bool isAddReferenceCount)
        {
#if EDITORLOAD && UNITY_EDITOR
			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFullName);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#elif RESOURCES
			string resourcesPath = assetFullName.Split('.')[0].Replace("Assets/Download/", string.Empty);

			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = Resources.Load(resourcesPath);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#else
            m_CurrAssetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetFullName);
            if (m_CurrAssetEntity == null) return;
            m_OnComplete = onComplete;
            m_OnUpdate = onUpdate;
            m_IsAddReferenceCount = isAddReferenceCount;

            LoadMainAsset();
#endif
        }

        /// <summary>
        /// 真正的加载主资源
        /// </summary>
        private void LoadMainAsset()
        {
            TaskGroup taskGroup = GameEntry.Task.CreateTaskGroup();

            //加载这个资源所依赖的资源包
            List<AssetDependsEntity> dependsAssetList = m_CurrAssetEntity.DependsAssetList;
            if (dependsAssetList != null)
            {
                foreach (AssetDependsEntity assetDependsEntity in dependsAssetList)
                {
                    AssetEntity assetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetDependsEntity.AssetFullName);
                    if (assetEntity != null)
                    {
                        if (!m_DependsAssetBundleNames.Add(assetEntity.AssetBundleName)) continue; //避免加载重复依赖文件
                        taskGroup.AddTask((taskRoutine) => GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(assetEntity.AssetBundleName, onComplete: (bundle) => taskRoutine.Leave()));
                    }
                    else
                    {
                        Debug.LogError("assetEntity==null, " + assetDependsEntity.AssetFullName);
                    }
                }
            }

            //加载主资源包
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(m_CurrAssetEntity.AssetBundleName, m_OnUpdate, onComplete: (AssetBundle bundle) =>
                {
                    m_MainAssetBundle = bundle;
                    taskRoutine.Leave();
                });
            });

            //任务组执行完毕
            taskGroup.OnComplete = () =>
            {
                if (m_MainAssetBundle == null)
                {
                    GameEntry.LogError("MainAssetBundle not exists " + m_CurrAssetEntity.AssetFullName);
                    m_OnComplete?.Invoke(null);
                    return;
                }
                GameEntry.Resource.ResourceLoaderManager.LoadAsset(m_CurrAssetEntity.AssetFullName, m_MainAssetBundle, onComplete: (UnityEngine.Object obj, bool isNew) =>
                {
                    //4.再次检查 很重要 不检查引用计数会出错
                    m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName, m_IsAddReferenceCount);
                    if (m_CurrResourceEntity != null)
                    {
                        m_OnComplete?.Invoke(m_CurrResourceEntity);
                        Reset();
                        return;
                    }

                    m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
                    m_CurrResourceEntity.IsAssetBundle = false;
                    m_CurrResourceEntity.ResourceName = m_CurrAssetEntity.AssetFullName;
                    m_CurrResourceEntity.Target = obj;

                    GameEntry.Pool.AssetPool.Register(m_CurrResourceEntity, m_IsAddReferenceCount);

                    m_OnComplete?.Invoke(m_CurrResourceEntity);
                    Reset();
                });
            };

            //Debug.LogError("任务开始运行");
            taskGroup.Run(true);
        }

        /// <summary>
        /// 重置
        /// </summary>
        private void Reset()
        {
            m_OnComplete = null;
            m_CurrAssetEntity = null;
            m_CurrResourceEntity = null;
            m_MainAssetBundle = null;
            m_DependsAssetBundleNames.Clear();
            GameEntry.Pool.EnqueueClassObject(this);
            m_IsAddReferenceCount = false;
        }
    }
}
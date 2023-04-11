using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

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
        /// 加载主资源(包括依赖)
        /// </summary>
        internal void LoadAction<T>(string assetFullName, Action<ResourceEntity> onComplete, Action<float> onUpdate) where T : Object
        {
#if EDITORLOAD && UNITY_EDITOR
            assetFullName = "Assets/Download/" + assetFullName;

            m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
            m_CurrResourceEntity.IsAssetBundle = false;
            m_CurrResourceEntity.ResourceName = assetFullName;
            m_CurrResourceEntity.Target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetFullName);
            onComplete?.Invoke(m_CurrResourceEntity);
            Reset();
#elif RESOURCES
            string[] temps = assetFullName.Split('.');

            if (temps.Length > 1)
            {
                StringBuilder str = StringHelper.PoolNew();
                for (int i = 0; i < temps.Length - 1; i++) str.Append(temps[i]);
                assetFullName = str.ToString();
                StringHelper.PoolDel(ref str);
            }

            m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
            m_CurrResourceEntity.IsAssetBundle = false;
            m_CurrResourceEntity.ResourceName = assetFullName;
            m_CurrResourceEntity.Target = Resources.Load<T>(assetFullName);
            if (m_CurrResourceEntity.Target == null) GameEntry.LogError(LogCategory.Resource, "资源加载失败==" + assetFullName);
            onComplete?.Invoke(m_CurrResourceEntity);
            Reset();
#else
            assetFullName = "Assets/Download/" + assetFullName;

            m_CurrAssetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetFullName);
            if (m_CurrAssetEntity == null) return;
            m_OnComplete = (retEntity) =>
            {
                onComplete?.Invoke(retEntity);
                Reset();
            };
            m_OnUpdate = onUpdate;

            LoadMainAssetAsync();
#endif
        }
        internal ResourceEntity Load(string assetFullName)
        {
#if EDITORLOAD && UNITY_EDITOR
            assetFullName = "Assets/Download/" + assetFullName;

            m_CurrResourceEntity = new ResourceEntity();
            m_CurrResourceEntity.IsAssetBundle = false;
            m_CurrResourceEntity.ResourceName = assetFullName;
            m_CurrResourceEntity.Target = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetFullName);
            //Reset();
            return m_CurrResourceEntity;
#elif RESOURCES
            string[] temps = assetFullName.Split('.');

            if (temps.Length > 1)
            {
                StringBuilder str = StringHelper.PoolNew();
                for (int i = 0; i < temps.Length - 1; i++) str.Append(temps[i]);
                assetFullName = str.ToString();
                StringHelper.PoolDel(ref str);
            }

            m_CurrResourceEntity = new ResourceEntity();
            m_CurrResourceEntity.IsAssetBundle = false;
            m_CurrResourceEntity.ResourceName = assetFullName;
            m_CurrResourceEntity.Target = Resources.Load(assetFullName);
            if (m_CurrResourceEntity.Target == null) GameEntry.LogError(LogCategory.Resource, "资源加载失败==" + assetFullName);
            //Reset();
            return m_CurrResourceEntity;
#else
            assetFullName = "Assets/Download/" + assetFullName;

            m_CurrAssetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetFullName);
            if (m_CurrAssetEntity == null) return null;
            LoadMainAsset();
            return m_CurrResourceEntity;
#endif
        }

        /// <summary>
        /// 真正的加载主资源
        /// </summary>
        private void LoadMainAssetAsync()
        {
            TaskGroup taskGroup = GameEntry.Task.CreateTaskGroup();

            bool IsSuffixScene = m_CurrAssetEntity.AssetFullName.IsSuffix(".unity");
            if (!IsSuffixScene)
            {
                //从分类资源池(AssetPool)中查找
                m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
                if (m_CurrResourceEntity != null)
                {
                    //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                    m_OnComplete?.Invoke(m_CurrResourceEntity);
                    return;
                }
            }

            //加载这个资源所依赖的资源包
            List<AssetDependsEntity> dependsAssetList = m_CurrAssetEntity.DependsAssetList;
            if (dependsAssetList != null)
            {
                foreach (AssetDependsEntity assetDependsEntity in dependsAssetList)
                {
                    AssetEntity assetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetDependsEntity.AssetFullName);
                    if (assetEntity != null)
                    {
                        if (!m_DependsAssetBundleNames.Add(assetEntity.AssetBundleName))
                        {
                            //避免加载重复依赖文件
                            GameEntry.LogError(LogCategory.Resource, "有重复依赖文件==" + assetEntity.AssetBundleName);
                            continue;
                        }
                        taskGroup.AddTask((taskRoutine) => GameEntry.Resource.ResourceLoaderManager.LoadAssetBundleAsync(assetEntity.AssetBundleName, onComplete: (bundle) => taskRoutine.Leave()));
                    }
                    else
                    {
                        YouYou.GameEntry.LogError(LogCategory.Resource, "assetEntity==null, " + assetDependsEntity.AssetFullName);
                    }
                }
            }

            //加载主资源包
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Resource.ResourceLoaderManager.LoadAssetBundleAsync(m_CurrAssetEntity.AssetBundleName, m_OnUpdate, onComplete: (AssetBundle bundle) =>
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
                    GameEntry.LogError(LogCategory.Resource, "MainAssetBundle not exists " + m_CurrAssetEntity.AssetFullName);
                    m_OnComplete?.Invoke(null);
                    return;
                }
                if (IsSuffixScene)
                {
                    m_OnComplete?.Invoke(null);
                    return;
                }
                //加载主资源
                GameEntry.Resource.ResourceLoaderManager.LoadAssetAsync(m_CurrAssetEntity.AssetFullName, m_MainAssetBundle, onComplete: (UnityEngine.Object obj, bool isNew) =>
                {
                    m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
                    if (m_CurrResourceEntity != null)
                    {
                        //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                        m_OnComplete?.Invoke(m_CurrResourceEntity);
                        return;
                    }

                    m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
                    m_CurrResourceEntity.IsAssetBundle = false;
                    m_CurrResourceEntity.ResourceName = m_CurrAssetEntity.AssetFullName;
                    m_CurrResourceEntity.Target = obj;
                    GameEntry.Pool.AssetPool.Register(m_CurrResourceEntity);
                    m_OnComplete?.Invoke(m_CurrResourceEntity);
                });
            };

            //YouYou.GameEntry.LogError("任务开始运行");
            taskGroup.Run(true);
        }
        private void LoadMainAsset()
        {
            bool IsSuffixScene = m_CurrAssetEntity.AssetFullName.IsSuffix(".unity");
            if (!IsSuffixScene)
            {
                //从分类资源池(AssetPool)中查找
                m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
                if (m_CurrResourceEntity != null)
                {
                    //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                    m_OnComplete?.Invoke(m_CurrResourceEntity);
                    return;
                }
            }

            //加载这个资源所依赖的资源包
            List<AssetDependsEntity> dependsAssetList = m_CurrAssetEntity.DependsAssetList;
            if (dependsAssetList != null)
            {
                foreach (AssetDependsEntity assetDependsEntity in dependsAssetList)
                {
                    AssetEntity assetEntity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetDependsEntity.AssetFullName);
                    if (assetEntity != null)
                    {
                        if (!m_DependsAssetBundleNames.Add(assetEntity.AssetBundleName))
                        {
                            //避免加载重复依赖文件
                            GameEntry.LogError(LogCategory.Resource, "有重复依赖文件==" + assetEntity.AssetBundleName);
                            continue;
                        }
                        GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(assetEntity.AssetBundleName);
                    }
                    else
                    {
                        GameEntry.LogError(LogCategory.Resource, "assetEntity==null, " + assetDependsEntity.AssetFullName);
                    }
                }
            }

            //加载主资源包
            m_MainAssetBundle = GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(m_CurrAssetEntity.AssetBundleName);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Resource, "MainAssetBundle not exists " + m_CurrAssetEntity.AssetFullName);
                m_OnComplete?.Invoke(null);
                return;
            }
            if (IsSuffixScene)
            {
                m_OnComplete?.Invoke(null);
                return;
            }

            //加载主资源
            Object obj = GameEntry.Resource.ResourceLoaderManager.LoadAsset(m_CurrAssetEntity.AssetFullName, m_MainAssetBundle);
            m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
            if (m_CurrResourceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                m_OnComplete?.Invoke(m_CurrResourceEntity);
                return;
            }

            m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
            m_CurrResourceEntity.IsAssetBundle = false;
            m_CurrResourceEntity.ResourceName = m_CurrAssetEntity.AssetFullName;
            m_CurrResourceEntity.Target = obj;
            GameEntry.Pool.AssetPool.Register(m_CurrResourceEntity);
            m_OnComplete?.Invoke(m_CurrResourceEntity);
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
        }
    }
}
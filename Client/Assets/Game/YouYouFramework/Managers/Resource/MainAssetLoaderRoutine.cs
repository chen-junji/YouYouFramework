using Main;
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
        /// 主资源包
        /// </summary>
        private AssetBundle m_MainAssetBundle;

        /// <summary>
        /// 异步加载主资源(包括依赖)
        /// </summary>
        internal static async ETTask<ResourceEntity> LoadAsyncStatic(string assetFullName, Action<float> onUpdate)
        {
            MainAssetLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<MainAssetLoaderRoutine>();
            ResourceEntity resourceEntity = await routine.LoadAsync(assetFullName, onUpdate);
            routine.Reset();
            return resourceEntity;
        }
        internal async ETTask<ResourceEntity> LoadAsync(string assetFullName, Action<float> onUpdate)
        {
            m_OnUpdate = onUpdate;

#if EDITORLOAD && UNITY_EDITOR
            m_CurrResourceEntity = ResourceEntity.Create(assetFullName, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetFullName));
#elif RESOURCES
            m_CurrResourceEntity = ResourceEntity.Create(assetFullName, Resources.Load(assetFullName));
#else
            m_CurrAssetEntity = GameEntry.Resource.GetAssetEntity(assetFullName);
            if (m_CurrAssetEntity == null) return null;

            //===================开始加载AB================
            bool IsSuffixScene = m_CurrAssetEntity.AssetFullName.IsSuffix(".unity");
            if (!IsSuffixScene)
            {
                //从分类资源池(AssetPool)中查找
                m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
                if (m_CurrResourceEntity != null)
                {
                    //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                    return m_CurrResourceEntity;
                }
            }

            //加载这个资源所依赖的资源包
            List<AssetDependsEntity> dependsAssetList = m_CurrAssetEntity.DependsAssetList;
            if (dependsAssetList != null)
            {
                for (int i = 0; i < dependsAssetList.Count; i++)
                {
                    await GameEntry.Resource.LoadAssetBundleAsync(dependsAssetList[i].AssetBundleName);
                }
            }

            //加载主资源包
            m_MainAssetBundle = await GameEntry.Resource.LoadAssetBundleAsync(m_CurrAssetEntity.AssetBundleName, m_OnUpdate);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Resource, "MainAssetBundle not exists " + m_CurrAssetEntity.AssetFullName);
                return null;
            }

            if (IsSuffixScene)
            {
                return null;
            }

            //加载主资源
            Object obj = await GameEntry.Resource.LoadAssetAsync(m_CurrAssetEntity.AssetFullName, m_MainAssetBundle);
            m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
            if (m_CurrResourceEntity != null)
            {
                return m_CurrResourceEntity;
            }
            else
            {
                m_CurrResourceEntity = ResourceEntity.Create(m_CurrAssetEntity.AssetFullName, obj);
                GameEntry.Pool.AssetPool.Register(m_CurrResourceEntity);
            }
#endif

            return m_CurrResourceEntity;
        }

        /// <summary>
        /// 同步加载主资源(包括依赖)
        /// </summary>
        internal static ResourceEntity LoadStatic(string assetFullName)
        {
            MainAssetLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<MainAssetLoaderRoutine>();
            ResourceEntity resourceEntity = routine.Load(assetFullName);
            routine.Reset();
            return resourceEntity;
        }
        internal ResourceEntity Load(string assetFullName)
        {
#if EDITORLOAD && UNITY_EDITOR
            m_CurrResourceEntity = ResourceEntity.Create(assetFullName, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetFullName));
#elif RESOURCES
            m_CurrResourceEntity = ResourceEntity.Create(assetFullName, Resources.Load(assetFullName));
#else
            m_CurrAssetEntity = GameEntry.Resource.GetAssetEntity(assetFullName);
            if (m_CurrAssetEntity == null) return null;

            //===================开始加载AB================
            bool IsSuffixScene = m_CurrAssetEntity.AssetFullName.IsSuffix(".unity");
            if (!IsSuffixScene)
            {
                //从分类资源池(AssetPool)中查找
                m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
                if (m_CurrResourceEntity != null)
                {
                    //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                    return m_CurrResourceEntity;
                }
            }

            //加载这个资源所依赖的资源包
            List<AssetDependsEntity> dependsAssetList = m_CurrAssetEntity.DependsAssetList;
            if (dependsAssetList != null)
            {
                for (int i = 0; i < dependsAssetList.Count; i++)
                {
                    GameEntry.Resource.LoadAssetBundle(dependsAssetList[i].AssetBundleName);
                }
            }

            //加载主资源包
            m_MainAssetBundle = GameEntry.Resource.LoadAssetBundle(m_CurrAssetEntity.AssetBundleName);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Resource, "MainAssetBundle not exists " + m_CurrAssetEntity.AssetFullName);
                return null;
            }
            if (IsSuffixScene)
            {
                return null;
            }

            //加载主资源
            Object obj = GameEntry.Resource.LoadAsset(m_CurrAssetEntity.AssetFullName, m_MainAssetBundle);
            m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
            if (m_CurrResourceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return m_CurrResourceEntity;
            }

            m_CurrResourceEntity = ResourceEntity.Create(m_CurrAssetEntity.AssetFullName, obj);
            GameEntry.Pool.AssetPool.Register(m_CurrResourceEntity);

#endif

            if (m_CurrResourceEntity.Target == null) GameEntry.LogError(LogCategory.Resource, "资源加载失败==" + assetFullName);
            return m_CurrResourceEntity;
        }

        /// <summary>
        /// 重置
        /// </summary>
        private void Reset()
        {
            m_CurrAssetEntity = null;
            m_CurrResourceEntity = null;
            m_MainAssetBundle = null;
            MainEntry.ClassObjectPool.Enqueue(this);
        }
    }
}
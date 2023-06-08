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

            //从分类资源池(AssetPool)中查找主资源
            m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
            if (m_CurrResourceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return m_CurrResourceEntity;
            }

            //加载主资源包和依赖资源包
            m_MainAssetBundle = await GameEntry.Resource.LoadMainAndDependAssetBundleAsync(m_CurrAssetEntity, m_OnUpdate);

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

            //从分类资源池(AssetPool)中查找主资源
            m_CurrResourceEntity = GameEntry.Pool.AssetPool.Spawn(m_CurrAssetEntity.AssetFullName);
            if (m_CurrResourceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return m_CurrResourceEntity;
            }

            //加载主资源包和依赖资源包
            m_MainAssetBundle = GameEntry.Resource.LoadMainAndDependAssetBundle(m_CurrAssetEntity);

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
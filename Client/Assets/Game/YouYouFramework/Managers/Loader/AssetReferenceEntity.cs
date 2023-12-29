using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// Asset引用计数实体
    /// </summary>
    public class AssetReferenceEntity
    {
        /// <summary>
        /// 资源完整路径，包含后缀名
        /// </summary>
        public string AssetFullPath;

        /// <summary>
        /// 关联目标
        /// </summary>
        public Object Target;

        /// <summary>
        /// 上次使用时间
        /// </summary>
        public float LastUseTime { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }


        /// <summary>
        /// 刷新最后使用时间
        /// </summary>
        public void RefeshLastUseTime()
        {
            LastUseTime = Time.time;
        }

        /// <summary>
        /// 引用计数+1
        /// </summary>
        public void ReferenceAdd()
        {
            ReferenceCount++;

#if ASSETBUNDLE
            if (ReferenceCount == 1)
            {
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(AssetFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceAdd();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceAdd();
                }
            }
#endif
        }
        /// <summary>
        /// 引用计数-1
        /// </summary>
        public void ReferenceRemove()
        {
            RefeshLastUseTime();
            ReferenceCount--;

#if ASSETBUNDLE
            if (ReferenceCount == 0)
            {
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(AssetFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceRemove();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceRemove();
                }
            }
#endif

            if (ReferenceCount < 0)
            {
                GameEntry.LogError(LogCategory.Loader, "Asset引用计数出错， ReferenceCount==" + ReferenceCount);
            }
        }

        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetInterval;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            AssetFullPath = null;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }

        public static AssetReferenceEntity Create(string assetFullPath, Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            AssetReferenceEntity referenceEntity = MainEntry.ClassObjectPool.Dequeue<AssetReferenceEntity>();
            referenceEntity.AssetFullPath = assetFullPath;
            referenceEntity.Target = obj;
            GameEntry.Pool.AssetPool.Register(referenceEntity);
            return referenceEntity;
        }
    }
}
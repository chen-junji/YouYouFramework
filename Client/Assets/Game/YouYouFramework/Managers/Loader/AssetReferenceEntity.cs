using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
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

        }
        /// <summary>
        /// 引用计数-1
        /// </summary>
        public void ReferenceRemove()
        {
            RefeshLastUseTime();
            ReferenceCount--;

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
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.ParamsSettings.PoolReleaseAssetInterval;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            //更新一下AssetBundle的引用计数
            if (MainEntry.IsAssetBundleMode)
            {
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(AssetFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Loader.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceRemove();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Loader.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceRemove();
                }
            }

            AssetFullPath = null;
            Target = null;
            GameEntry.Pool.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }

        public static AssetReferenceEntity Create(string assetFullPath, Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            AssetReferenceEntity referenceEntity = GameEntry.Pool.ClassObjectPool.Dequeue<AssetReferenceEntity>();
            referenceEntity.AssetFullPath = assetFullPath;
            referenceEntity.Target = obj;
            GameEntry.Loader.MainAssetPool.Register(referenceEntity);

            //更新一下AssetBundle的引用计数
            if (MainEntry.IsAssetBundleMode)
            {
                AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(assetFullPath);
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Loader.AssetBundlePool.Spawn(assetEntity.AssetBundleFullPath);
                assetBundleEntity.ReferenceAdd();
                for (int i = 0; i < assetEntity.DependsAssetBundleList.Count; i++)
                {
                    AssetBundleReferenceEntity dependAssetBundleEntity = GameEntry.Loader.AssetBundlePool.Spawn(assetEntity.DependsAssetBundleList[i]);
                    dependAssetBundleEntity.ReferenceAdd();
                }
            }

            return referenceEntity;
        }
    }
}
using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// AssetBundle引用计数实体
    /// </summary>
    public class AssetBundleReferenceEntity
    {
        /// <summary>
        /// 资源包路径
        /// </summary>
        public string AssetBundlePath;

        /// <summary>
        /// 关联目标
        /// </summary>
        public AssetBundle Target;

        /// <summary>
        /// 上次使用时间
        /// </summary>
        public float LastUseTime { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }


        /// <summary>
        /// 对象取池
        /// </summary>
        public void Spawn()
        {
            LastUseTime = Time.time;

            //如果是锁定资源包 不释放
            if (GameEntry.Pool.CheckAssetBundleIsLock(AssetBundlePath))
            {
                ReferenceCount = 1;
            }
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public void Unspawn()
        {
            LastUseTime = Time.time;
        }

        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval;
        }

        public static AssetBundleReferenceEntity Create(string path, AssetBundle target)
        {
            AssetBundleReferenceEntity assetBundleEntity = MainEntry.ClassObjectPool.Dequeue<AssetBundleReferenceEntity>();
            assetBundleEntity.AssetBundlePath = path;
            assetBundleEntity.Target = target;
            GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);
            return assetBundleEntity;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            AssetBundle bundle = Target;
            bundle.Unload(false);

            AssetBundlePath = null;
            ReferenceCount = 0;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }
    }
}
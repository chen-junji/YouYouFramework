using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class AssetBundleEntity
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

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
        /// 对象取池(reference==true则引用计数+1)
        /// </summary>
        public void Spawn(bool reference)
        {
            LastUseTime = Time.time;

            //如果是锁定资源包 不释放
            if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
            {
                ReferenceCount = 1;
            }
        }

        /// <summary>
        /// 对象回池(reference==true则引用计数-1)
        /// </summary>
        public void Unspawn(bool reference)
        {
#if ASSETBUNDLE
            LastUseTime = Time.time;
#else
            Target = null;
            MainEntry.ClassObjectPool.Enqueue(this);
#endif
        }

        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            AssetBundle bundle = Target;
            bundle.Unload(false);

            ResourceName = null;
            ReferenceCount = 0;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }
    }
}
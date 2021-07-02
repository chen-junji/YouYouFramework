using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源实体(AssetBundle和Asset实体)
    /// </summary>
    public class ResourceEntity
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

        /// <summary>
        /// 是否AssetBundle
        /// </summary>
        public bool IsAssetBundle;

        /// <summary>
        /// 关联目标
        /// </summary>
        public object Target;

        /// <summary>
        /// 上次使用时间
        /// </summary>
        public float LastUseTime { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }

        /// <summary>
        /// 依赖的资源实体链表
        /// </summary>
        public LinkedList<ResourceEntity> DependsResourceList { private set; get; }


        public ResourceEntity()
        {
            DependsResourceList = new LinkedList<ResourceEntity>();
        }

        /// <summary>
        /// 对象取池
        /// </summary>
        public void Spawn(bool reference)
        {
            LastUseTime = Time.time;

            if (!IsAssetBundle)
            {
                if (reference) ReferenceCount++;
            }
            else
            {
                //如果是锁定资源包 不释放
                if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
                {
                    ReferenceCount = 1;
                }
            }
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public void Unspawn()
        {
            LastUseTime = Time.time;

            if (!IsAssetBundle)
            {
                ReferenceCount--;
                if (ReferenceCount < 0) ReferenceCount = 0;
            }
        }
        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > (IsAssetBundle ? GameEntry.Pool.ReleaseAssetBundleInterval : GameEntry.Pool.ReleaseAssetInterval);
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            if (IsAssetBundle)
            {
                AssetBundle bundle = Target as AssetBundle;
                bundle.Unload(false);
            }

            ResourceName = null;
            ReferenceCount = 0;
            Target = null;

            DependsResourceList.Clear(); //把自己依赖的资源实体清空
            GameEntry.Pool.EnqueueClassObject(this); //把这个资源实体回池
        }
    }
}
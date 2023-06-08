using Main;
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
        /// 对象取池(reference==true则引用计数+1)
        /// </summary>
        public void Spawn(bool reference)
        {
            LastUseTime = Time.time;

            if (reference) ReferenceCount++;
        }

        /// <summary>
        /// 对象回池(reference==true则引用计数-1)
        /// </summary>
        public void Unspawn(bool reference)
        {
            LastUseTime = Time.time;

            if (reference) ReferenceCount--;
            if (ReferenceCount < 0) ReferenceCount = 0;
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
            ResourceName = null;
            ReferenceCount = 0;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }

        public static ResourceEntity Create(string name, Object obj)
        {
            ResourceEntity resourceEntity = MainEntry.ClassObjectPool.Dequeue<ResourceEntity>();
            resourceEntity.ResourceName = name;
            resourceEntity.Target = obj;
            resourceEntity.Spawn(false);
            GameEntry.Pool.AssetPool.Register(resourceEntity);
            return resourceEntity;
        }
    }
}
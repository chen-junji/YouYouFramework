using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouMain;

namespace YouYouFramework
{
    /// <summary>
    /// 主资源池(只做主资源的引用计数)
    /// </summary>
    public class AssetPool
    {
#if UNITY_EDITOR
        /// <summary>
        /// 在监视面板显示的信息
        /// </summary>
        public Dictionary<string, AssetReferenceEntity> InspectorDic = new Dictionary<string, AssetReferenceEntity>();
#endif

        /// <summary>
        /// 资源池字典
        /// </summary>
        private Dictionary<string, AssetReferenceEntity> m_AssetDic;

        /// <summary>
        /// 需要移除的Key链表
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// 下次释放Asset池运行时间
        /// </summary>
        public float ReleaseNextRunTime { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssetPool()
        {
            m_AssetDic = new Dictionary<string, AssetReferenceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();

            ReleaseNextRunTime = Time.time;
        }
        internal void OnUpdate()
        {
            if (Time.time > ReleaseNextRunTime + MainEntry.ParamsSettings.PoolReleaseAssetInterval)
            {
                ReleaseNextRunTime = Time.time;
                Release();
                //GameEntry.Log(LogCategory.Normal, "释放Asset池");
            }
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(AssetReferenceEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.AssetFullPath, entity);
#endif
            m_AssetDic.Add(entity.AssetFullPath, entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        public AssetReferenceEntity Spawn(string resourceName)
        {
            if (m_AssetDic.TryGetValue(resourceName, out AssetReferenceEntity referenceEntity))
            {
                return referenceEntity;
            }
            return null;
        }

        /// <summary>
        /// 释放资源池中可释放资源
        /// </summary>
        public void Release()
        {
            var enumerator = m_AssetDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetReferenceEntity referenceEntity = enumerator.Current.Value;
                if (referenceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    InspectorDic.Remove(referenceEntity.AssetFullPath);
#endif
                    m_NeedRemoveKeyList.AddFirst(referenceEntity.AssetFullPath);
                    referenceEntity.Release();
                }
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_AssetDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }

    }
}
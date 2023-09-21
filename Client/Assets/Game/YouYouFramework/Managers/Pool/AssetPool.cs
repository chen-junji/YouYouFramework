using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源池
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
        /// 资源池名称
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// 资源池字典
        /// </summary>
        private Dictionary<string, AssetReferenceEntity> m_AssetDic;

        /// <summary>
        /// 需要移除的Key链表
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolName">资源池名称</param>
        public AssetPool(string poolName)
        {
            PoolName = poolName;
            m_AssetDic = new Dictionary<string, AssetReferenceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(AssetReferenceEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.AssetPath, entity);
#endif
            entity.Spawn(false);
            m_AssetDic.Add(entity.AssetPath, entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        public AssetReferenceEntity Spawn(string resourceName)
        {
            if (m_AssetDic.TryGetValue(resourceName, out AssetReferenceEntity referenceEntity))
            {
                referenceEntity.Spawn(false);
            }
            return referenceEntity;
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
                    InspectorDic.Remove(referenceEntity.AssetPath);
#endif
                    m_NeedRemoveKeyList.AddFirst(referenceEntity.AssetPath);
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
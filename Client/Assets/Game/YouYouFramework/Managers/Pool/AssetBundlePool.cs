using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    public class AssetBundlePool
    {
#if UNITY_EDITOR
        /// <summary>
        /// 在监视面板显示的信息
        /// </summary>
        public Dictionary<string, AssetBundleReferenceEntity> InspectorDic = new Dictionary<string, AssetBundleReferenceEntity>();
#endif

        /// <summary>
        /// 资源池名称
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// 资源池字典
        /// </summary>
        private Dictionary<string, AssetBundleReferenceEntity> m_AssetBundleDic;

        /// <summary>
        /// 需要移除的Key链表
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolName">资源池名称</param>
        public AssetBundlePool(string poolName)
        {
            PoolName = poolName;
            m_AssetBundleDic = new Dictionary<string, AssetBundleReferenceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(AssetBundleReferenceEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.AssetBundlePath, entity);
#endif
            m_AssetBundleDic.Add(entity.AssetBundlePath, entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        public AssetBundleReferenceEntity Spawn(string resourceName)
        {
            if (m_AssetBundleDic.TryGetValue(resourceName, out AssetBundleReferenceEntity abReferenceEntity))
            {
                return abReferenceEntity;
            }
            return null;
        }

        /// <summary>
        /// 释放资源池中可释放资源
        /// </summary>
        public void Release()
        {
            var enumerator = m_AssetBundleDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleReferenceEntity abReferenceEntity = enumerator.Current.Value;
                if (abReferenceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    if (InspectorDic.ContainsKey(abReferenceEntity.AssetBundlePath))
                    {
                        InspectorDic.Remove(abReferenceEntity.AssetBundlePath);
                    }
#endif
                    m_NeedRemoveKeyList.AddFirst(abReferenceEntity.AssetBundlePath);
                    abReferenceEntity.Release();
                }
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_AssetBundleDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }

    }
}
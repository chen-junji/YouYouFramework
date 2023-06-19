using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public class AssetBundlePool : MonoBehaviour
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
        private Dictionary<string, AssetBundleReferenceEntity> m_ResourceDic;

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
            m_ResourceDic = new Dictionary<string, AssetBundleReferenceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(AssetBundleReferenceEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.ResourceName, entity);
#endif
            m_ResourceDic.Add(entity.ResourceName, entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        public AssetBundleReferenceEntity Spawn(string resourceName)
        {
            if (m_ResourceDic.TryGetValue(resourceName, out AssetBundleReferenceEntity abReferenceEntity))
            {
                abReferenceEntity.Spawn();
            }
            return abReferenceEntity;
        }

        /// <summary>
        /// 释放资源池中可释放资源
        /// </summary>
        public void Release()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleReferenceEntity abReferenceEntity = enumerator.Current.Value;
                if (abReferenceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    if (InspectorDic.ContainsKey(abReferenceEntity.ResourceName))
                    {
                        InspectorDic.Remove(abReferenceEntity.ResourceName);
                    }
#endif
                    m_NeedRemoveKeyList.AddFirst(abReferenceEntity.ResourceName);
                    abReferenceEntity.Release();
                }
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_ResourceDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }

        /// <summary>
        /// 释放池内所有资源
        /// </summary>
        public void ReleaseAll()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleReferenceEntity abReferenceEntity = enumerator.Current.Value;
#if UNITY_EDITOR
                if (InspectorDic.ContainsKey(abReferenceEntity.ResourceName))
                {
                    InspectorDic.Remove(abReferenceEntity.ResourceName);
                }
#endif
                m_NeedRemoveKeyList.AddFirst(abReferenceEntity.ResourceName);
                abReferenceEntity.Release();
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_ResourceDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }
    }
}
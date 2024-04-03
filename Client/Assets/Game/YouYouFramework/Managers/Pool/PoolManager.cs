using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using YouYouMain;

namespace YouYouFramework
{
    /// <summary>
    /// 池管理器
    /// </summary>
    public class PoolManager
    {
        /// <summary>
        /// 游戏物体对象池
        /// </summary>
        public GameObjectPool GameObjectPool { get; private set; }

        internal PoolManager()
        {
            GameObjectPool = new GameObjectPool();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {

            InitClassReside();

            GameObjectPool.Init();
        }

        //============================

        /// <summary>
        /// 初始化常用类常驻数量
        /// </summary>
        private void InitClassReside()
        {
            MainEntry.ClassObjectPool.SetResideCount<HttpRoutine>(3);
            MainEntry.ClassObjectPool.SetResideCount<Dictionary<string, object>>(3);
            MainEntry.ClassObjectPool.SetResideCount<AssetBundleLoaderRoutine>(10);
            MainEntry.ClassObjectPool.SetResideCount<AssetLoaderRoutine>(10);
            MainEntry.ClassObjectPool.SetResideCount<AssetReferenceEntity>(10);
            MainEntry.ClassObjectPool.SetResideCount<AssetBundleReferenceEntity>(10);
        }

        #region 变量对象池

        /// <summary>
        /// 变量对象池锁
        /// </summary>
        private object m_VarObjectLock = new object();

#if UNITY_EDITOR
        /// <summary>
        /// 在监视面板显示的信息
        /// </summary>
        public Dictionary<Type, int> VarObjectInspectorDic = new Dictionary<Type, int>();
#endif

        /// <summary>
        /// 取出一个变量对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T DequeueVarObject<T>() where T : VariableBase, new()
        {
            lock (m_VarObjectLock)
            {
                T item = MainEntry.ClassObjectPool.Dequeue<T>();
#if UNITY_EDITOR
                Type t = item.GetType();
                if (VarObjectInspectorDic.ContainsKey(t))
                {
                    VarObjectInspectorDic[t]++;
                }
                else
                {
                    VarObjectInspectorDic[t] = 1;
                }
#endif
                return item;
            }
        }

        /// <summary>
        /// 变量对象回池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void EnqueueVarObject<T>(T item) where T : VariableBase
        {
            lock (m_VarObjectLock)
            {
                MainEntry.ClassObjectPool.Enqueue(item);
#if UNITY_EDITOR
                Type t = item.GetType();
                if (VarObjectInspectorDic.ContainsKey(t))
                {
                    VarObjectInspectorDic[t]--;
                    if (VarObjectInspectorDic[t] == 0)
                    {
                        VarObjectInspectorDic.Remove(t);
                    }
                }
#endif
            }
        }

        #endregion

        internal void OnUpdate()
        {
        }

    }
}
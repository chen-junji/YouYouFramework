using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Main;

namespace YouYou
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
        /// <summary>
        /// 资源包池
        /// </summary>
        public AssetBundlePool AssetBundlePool { get; private set; }
        /// <summary>
        /// 主资源池
        /// </summary>
        public ResourcePool AssetPool { get; private set; }

        internal PoolManager()
        {
            GameObjectPool = new GameObjectPool();

            AssetBundlePool = new AssetBundlePool("AssetBundlePool");
            m_InstanceResourceDic = new Dictionary<int, ResourceEntity>();
            AssetPool = new ResourcePool("AssetPool");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
            ReleaseClassObjectInterval = Main.MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseClassObjectInterval, Main.MainEntry.CurrDeviceGrade);
            ReleaseAssetBundleInterval = Main.MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetBundleInterval, Main.MainEntry.CurrDeviceGrade);
            ReleaseAssetInterval = Main.MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetInterval, Main.MainEntry.CurrDeviceGrade);

            ReleaseClassObjectNextRunTime = Time.time;
            ReleaseAssetBundleNextRunTime = Time.time;
            ReleaseAssetNextRunTime = Time.time;

            m_LockedAssetBundleLength = GameEntry.Instance.LockedAssetBundle.Length;
            InitClassReside();

            GameObjectPool.Init();
        }

        //============================


        /// <summary>
        /// 锁定的资源包数组长度
        /// </summary>
        private int m_LockedAssetBundleLength;

        /// <summary>
        /// 检查资源包是否锁定
        /// </summary>
        /// <param name="assetBundleName">资源包名称</param>
        /// <returns></returns>
        public bool CheckAssetBundleIsLock(string assetBundleName)
        {
            for (int i = 0; i < m_LockedAssetBundleLength; i++)
            {
                if (GameEntry.Instance.LockedAssetBundle[i].Equals(assetBundleName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 初始化常用类常驻数量
        /// </summary>
        private void InitClassReside()
        {
            MainEntry.ClassObjectPool.SetResideCount<HttpRoutine>(3);
            MainEntry.ClassObjectPool.SetResideCount<Dictionary<string, object>>(3);
            MainEntry.ClassObjectPool.SetResideCount<AssetBundleLoaderRoutine>(10);
            MainEntry.ClassObjectPool.SetResideCount<AssetLoaderRoutine>(10);
            MainEntry.ClassObjectPool.SetResideCount<ResourceEntity>(10);
            //MainEntry.ClassObjectPool.SetResideCount<AssetBundleEntity>(10);
            MainEntry.ClassObjectPool.SetResideCount<MainAssetLoaderRoutine>(30);
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

        /// <summary>
        /// 释放类对象池间隔
        /// </summary>
        public int ReleaseClassObjectInterval { get; private set; }
        /// <summary>
        /// 下次释放类对象运行时间
        /// </summary>
        public float ReleaseClassObjectNextRunTime { get; private set; }

        /// <summary>
        /// 释放AssetBundle池间隔
        /// </summary>
        public int ReleaseAssetBundleInterval { get; private set; }
        /// <summary>
        /// 下次释放AssetBundle池运行时间
        /// </summary>
        public float ReleaseAssetBundleNextRunTime { get; private set; }

        /// <summary>
        /// 释放Asset池间隔
        /// </summary>
        public int ReleaseAssetInterval { get; private set; }
        /// <summary>
        /// 下次释放Asset池运行时间
        /// </summary>
        public float ReleaseAssetNextRunTime { get; private set; }

        internal void OnUpdate()
        {
            if (Time.time > ReleaseClassObjectNextRunTime + ReleaseClassObjectInterval)
            {
                ReleaseClassObjectNextRunTime = Time.time;
                MainEntry.ClassObjectPool.Release();
                //GameEntry.Log(LogCategory.Normal, "释放类对象池");
            }


            if (Time.time > ReleaseAssetBundleNextRunTime + ReleaseAssetBundleInterval)
            {
                ReleaseAssetBundleNextRunTime = Time.time;

#if ASSETBUNDLE
                AssetBundlePool.Release();
                //GameEntry.Log(LogCategory.Normal, "释放AssetBundle池");
#endif
            }

            if (Time.time > ReleaseAssetNextRunTime + ReleaseAssetInterval)
            {
                ReleaseAssetNextRunTime = Time.time;

#if ASSETBUNDLE
                AssetPool.Release();
                //GameEntry.Log(LogCategory.Normal, "释放Asset池");
#endif
            }
        }

        #region 游戏物体对象池
        /// <summary>
        /// 克隆出来的实例资源字典
        /// </summary>
        private Dictionary<int, ResourceEntity> m_InstanceResourceDic;


        /// <summary>
        /// 把克隆出来的资源 加入实例资源池
        /// </summary>
        public void RegisterInstanceResource(int instanceId, ResourceEntity resourceEntity)
        {
            //YouYou.GameEntry.LogError("注册到实例字典instanceId=" + instanceId);
            m_InstanceResourceDic[instanceId] = resourceEntity;
            resourceEntity.Spawn(true);
        }

        /// <summary>
        /// 释放实例资源, 从实例字典Remove
        /// </summary>
        public void ReleaseInstanceResource(int instanceId)
        {
            //YouYou.GameEntry.LogError("释放实例资源instanceId=" + instanceId);
            if (m_InstanceResourceDic.TryGetValue(instanceId, out ResourceEntity resourceEntity))
            {
                resourceEntity.Unspawn(true);
                m_InstanceResourceDic.Remove(instanceId);
            }
        }
        #endregion
    }
}
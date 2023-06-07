using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 资源加载管理器
    /// </summary>
    public class ResourceLoaderManager
    {
        /// <summary>
        /// 资源信息字典
        /// </summary>
        private Dictionary<string, AssetEntity> m_AssetInfoDic;

        /// <summary>
        /// 资源包加载器链表
        /// </summary>
        private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

        /// <summary>
        /// 资源加载器链表
        /// </summary>
        private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;

        public ResourceLoaderManager()
        {
            m_AssetInfoDic = new Dictionary<string, AssetEntity>();
            m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
            m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();
        }
        internal void Init()
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;
        }
        internal void OnUpdate()
        {
            for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
            {
                curr.Value.OnUpdate();
            }

            for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
            {
                curr.Value.OnUpdate();
            }
        }

        #region InitAssetInfo 初始化资源信息
        private Action m_InitAssetInfoComplete;
        /// <summary>
        /// 初始化资源信息
        /// </summary>
        internal void InitAssetInfo(Action initAssetInfoComplete)
        {
            m_InitAssetInfoComplete = initAssetInfoComplete;

            byte[] buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(YFConstDefine.AssetInfoName);
            if (buffer == null)
            {
                //如果可写区没有 那么就从只读区获取
                MainEntry.ResourceManager.StreamingAssetsManager.ReadAssetBundleAsync(YFConstDefine.AssetInfoName, async (byte[] buff) =>
                {
                    if (buff == null)
                    {
                        //如果只读区也没有,从CDN读取
                        string url = string.Format("{0}{1}", MainEntry.Data.CurrChannelConfig.RealSourceUrl, YFConstDefine.AssetInfoName);
                        HttpCallBackArgs args = await GameEntry.Http.GetArgsAsync(url, false);
                        if (!args.HasError)
                        {
                            GameEntry.Log(LogCategory.Resource, "从CDN初始化资源信息");
                            InitAssetInfo(args.Data);
                        }
                    }
                    else
                    {
                        GameEntry.Log(LogCategory.Resource, "从只读区初始化资源信息");
                        InitAssetInfo(buff);
                    }
                });
            }
            else
            {
                GameEntry.Log(LogCategory.Resource, "从可写区初始化资源信息");
                InitAssetInfo(buffer);
            }
        }

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        /// <param name="buffer"></param>
        private void InitAssetInfo(byte[] buffer)
        {
            buffer = ZlibHelper.DeCompressBytes(buffer);//解压

            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
            int len = ms.ReadInt();
            int depLen = 0;
            for (int i = 0; i < len; i++)
            {
                AssetEntity entity = new AssetEntity();
                entity.AssetFullName = ms.ReadUTF8String();
                entity.AssetBundleName = ms.ReadUTF8String();

                //GameEntry.Log("entity.AssetBundleName=" + entity.AssetBundleName);
                //GameEntry.Log("entity.AssetFullName=" + entity.AssetFullName);

                depLen = ms.ReadInt();
                if (depLen > 0)
                {
                    entity.DependsAssetList = new List<AssetDependsEntity>(depLen);
                    for (int j = 0; j < depLen; j++)
                    {
                        AssetDependsEntity assetDepends = new AssetDependsEntity();
                        //assetDepends.AssetFullName = ms.ReadUTF8String();
                        assetDepends.AssetBundleName = ms.ReadUTF8String();
                        entity.DependsAssetList.Add(assetDepends);
                    }
                }

                m_AssetInfoDic[entity.AssetFullName] = entity;
            }

            m_InitAssetInfoComplete?.Invoke();
        }

        /// <summary>
        /// 根据资源路径获取资源信息
        /// </summary>
        /// <param name="assetFullName">资源路径</param>
        /// <returns></returns>
        internal AssetEntity GetAssetEntity(string assetFullName)
        {
            AssetEntity entity = null;
            if (m_AssetInfoDic.TryGetValue(assetFullName, out entity))
            {
                return entity;
            }
            GameEntry.LogError(LogCategory.Resource, "资源不存在, assetFullName=>{0}", assetFullName);
            return null;
        }
        #endregion

        #region LoadAssetBundle 加载资源包
        /// <summary>
        /// 加载中的Bundle
        /// </summary>
        private TaskGroup AssetBundleTaskGroup = new TaskGroup();
        /// <summary>
        /// 加载资源包
        /// </summary>
        public void LoadAssetBundleAction(string assetbundlePath, Action<float> onUpdate = null, Action<AssetBundle> onComplete = null)
        {
            //使用TaskGroup, 加入异步加载队列, 防止高并发导致的重复加载
            AssetBundleTaskGroup.AddTask((taskRoutine) =>
            {
                //1.判断资源包是否存在于AssetBundlePool
                AssetBundleEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
                if (assetBundleEntity != null)
                {
                    //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                    taskRoutine.Leave();
                    onComplete?.Invoke(assetBundleEntity.Target);
                    return;
                }

                AssetBundleLoaderRoutine loadRoutine = MainEntry.ClassObjectPool.Dequeue<AssetBundleLoaderRoutine>();

                //加入链表开始Update()
                m_AssetBundleLoaderList.AddLast(loadRoutine);
                //资源包加载 监听回调
                loadRoutine.OnAssetBundleCreateUpdate = onUpdate;
                loadRoutine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
                {
                    //资源包注册到资源池
                    assetBundleEntity = AssetBundleEntity.Create(assetbundlePath, assetbundle);
                    GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);

                    taskRoutine.Leave();
                    onComplete?.Invoke(assetBundleEntity.Target);

                    m_AssetBundleLoaderList.Remove(loadRoutine);
                    MainEntry.ClassObjectPool.Enqueue(loadRoutine);
                };
                //加载资源包
                loadRoutine.LoadAssetBundleAsync(assetbundlePath);
            });
            AssetBundleTaskGroup.Run();
        }

        public async ETTask<AssetBundle> LoadAssetBundleAsync(string assetbundlePath, Action<float> onUpdate = null)
        {
            ETTask<AssetBundle> task = ETTask<AssetBundle>.Create();
            LoadAssetBundleAction(assetbundlePath, onUpdate, (ab) =>
            {
                task.SetResult(ab);
            });
            return await task;
        }

        public AssetBundle LoadAssetBundle(string assetbundlePath)
        {
            //1.判断资源包是否存在于AssetBundlePool
            AssetBundleEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
            if (assetBundleEntity != null)
            {
                //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                return assetBundleEntity.Target;
            }

            //加载资源包
            AssetBundleLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<AssetBundleLoaderRoutine>();
            AssetBundle assetbundle = routine.LoadAssetBundle(assetbundlePath);

            //资源包注册到资源池
            assetBundleEntity = AssetBundleEntity.Create(assetbundlePath, assetbundle);
            GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);
            MainEntry.ClassObjectPool.Enqueue(routine);

            return assetbundle;
        }
        #endregion

        #region LoadAsset 从资源包中加载资源
        /// <summary>
        /// 加载中的Asset
        /// </summary>
        private TaskGroup AssetTaskGroup = new TaskGroup();
        /// <summary>
        /// 异步加载
        /// </summary>
        public void LoadAssetAction(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null, Action<Object> onComplete = null)
        {
            //使用TaskGroup, 加入异步加载队列, 防止高并发导致的重复加载
            AssetTaskGroup.AddTask((taskRoutine) =>
            {
                AssetLoaderRoutine routine = MainEntry.ClassObjectPool.Dequeue<AssetLoaderRoutine>();

                //加入链表开始循环
                m_AssetLoaderList.AddLast(routine);

                //资源加载 进行中 回调
                routine.OnAssetUpdate = onUpdate;
                //资源加载 结果 回调
                routine.OnLoadAssetComplete = (Object obj) =>
                {
                    taskRoutine.Leave();
                    onComplete?.Invoke(obj);

                    //结束循环 回池
                    m_AssetLoaderList.Remove(routine);
                    MainEntry.ClassObjectPool.Enqueue(routine);
                };
                //加载资源
                routine.LoadAssetAsync(assetName, assetBundle);
            });
            AssetTaskGroup.Run();
        }
        public async ETTask<Object> LoadAssetAsync(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null)
        {
            ETTask<Object> task = ETTask<Object>.Create();
            LoadAssetAction(assetName, assetBundle, onUpdate, (obj) =>
            {
                task.SetResult(obj);
            });
            return await task;
        }

        public Object LoadAsset(string assetName, AssetBundle assetBundle)
        {
            return assetBundle.LoadAsset(assetName);
        }
        #endregion

        #region LoadMainAsset 加载主资源(自动加载依赖)
        public async ETTask<T> LoadMainAssetAsync<T>(string assetFullName, Action<float> onUpdate = null) where T : Object
        {
            ResourceEntity resEntity = await MainAssetLoaderRoutine.LoadAsyncStatic(assetFullName, onUpdate);
            return resEntity.Target as T;
        }
        public async ETTask<ResourceEntity> LoadMainAssetAsync(string assetFullName, Action<float> onUpdate = null)
        {
            ResourceEntity resEntity = await MainAssetLoaderRoutine.LoadAsyncStatic(assetFullName, onUpdate);
            return resEntity;
        }

        public T LoadMainAsset<T>(string assetFullName) where T : Object
        {
            return MainAssetLoaderRoutine.LoadStatic(assetFullName).Target as T;
        }
        public ResourceEntity LoadMainAsset(string assetFullName)
        {
            return MainAssetLoaderRoutine.LoadStatic(assetFullName);
        }
        #endregion
    }
}
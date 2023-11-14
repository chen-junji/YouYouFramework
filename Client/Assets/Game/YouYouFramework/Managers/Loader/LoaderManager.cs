using Main;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

namespace YouYou
{
    /// <summary>
    /// 资源加载 管理器
    /// </summary>
    public class LoaderManager
    {
        /// <summary>
        /// 资源依赖信息 管理器
        /// </summary>
        public AssetInfoManager AssetInfo { get; private set; }

        /// <summary>
        /// 资源包加载器链表
        /// </summary>
        private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

        /// <summary>
        /// 资源加载器链表
        /// </summary>
        private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;

        public LoaderManager()
        {
            AssetInfo = new AssetInfoManager();
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


        #region LoadAssetBundle 加载资源包
        /// <summary>
        /// 加载中的Bundle
        /// </summary>
        private TaskGroup AssetBundleTaskGroup = new TaskGroup();
        /// <summary>
        /// 加载资源包
        /// </summary>
        public void LoadAssetBundleAction(string assetbundlePath, Action<AssetBundle> onComplete = null, Action<float> onUpdate = null,Action<float>onDownloadUpdate = null)
        {
            //使用TaskGroup, 加入异步加载队列, 防止高并发导致的重复加载
            AssetBundleTaskGroup.AddTask((taskRoutine) =>
            {
                //判断资源包是否存在于AssetBundlePool
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
                if (assetBundleEntity != null)
                {
                    //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                    onComplete?.Invoke(assetBundleEntity.Target);
                    taskRoutine.Leave();
                    return;
                }

                AssetBundleLoaderRoutine loadRoutine = AssetBundleLoaderRoutine.Create();

                //加入链表开始Update()
                m_AssetBundleLoaderList.AddLast(loadRoutine);
                //资源包加载 监听回调
                loadRoutine.OnAssetBundleCreateUpdate = onUpdate;
                loadRoutine.OnAssetBundleDownloadUpdate = onDownloadUpdate;
                loadRoutine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
                {
                    //资源包注册到资源池
                    AssetBundleReferenceEntity.Create(assetbundlePath, assetbundle);

                    taskRoutine.Leave();
                    onComplete?.Invoke(assetbundle);

                    m_AssetBundleLoaderList.Remove(loadRoutine);
                };
                //加载资源包
                loadRoutine.LoadAssetBundleAsync(assetbundlePath);
            });
            AssetBundleTaskGroup.Run();
        }

        public UniTask<AssetBundle> LoadAssetBundleAsync(string assetbundlePath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            var task = new UniTaskCompletionSource<AssetBundle>();
            LoadAssetBundleAction(assetbundlePath, (ab) =>
            {
                task.TrySetResult(ab);
            }, onUpdate, onDownloadUpdate);
            return task.Task;
        }
        /// <summary>
        /// 加载主资源包和依赖资源包
        /// </summary>
        public async UniTask<AssetBundle> LoadAssetBundleMainAndDependAsync(string assetFullName, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(assetFullName);
            if (assetEntity == null) return null;

            //加载这个资源所依赖的资源包
            List<string> dependsAssetList = assetEntity.DependsAssetBundleList;
            if (dependsAssetList != null)
            {
                for (int i = 0; i < dependsAssetList.Count; i++)
                {
                    await GameEntry.Loader.LoadAssetBundleAsync(dependsAssetList[i]);
                }
            }

            //加载主资源包
            AssetBundle m_MainAssetBundle = await GameEntry.Loader.LoadAssetBundleAsync(assetEntity.AssetBundleName, onUpdate, onDownloadUpdate);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Loader, "MainAssetBundle not exists " + assetEntity.AssetFullName);
                return null;
            }
            return m_MainAssetBundle;
        }

        public AssetBundle LoadAssetBundleMainAndDepend(string assetFullName)
        {
            AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(assetFullName);
            if (assetEntity == null) return null;

            //加载这个资源所依赖的资源包
            List<string> dependsAssetList = assetEntity.DependsAssetBundleList;
            if (dependsAssetList != null)
            {
                for (int i = 0; i < dependsAssetList.Count; i++)
                {
                    GameEntry.Loader.LoadAssetBundle(dependsAssetList[i]);
                }
            }

            //加载主资源包
            AssetBundle m_MainAssetBundle = GameEntry.Loader.LoadAssetBundle(assetEntity.AssetBundleName);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Loader, "MainAssetBundle not exists " + assetEntity.AssetFullName);
                return null;
            }
            return m_MainAssetBundle;
        }
        public AssetBundle LoadAssetBundle(string assetbundlePath)
        {
            //判断资源包是否存在于AssetBundlePool
            AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
            if (assetBundleEntity != null)
            {
                //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                return assetBundleEntity.Target;
            }

            //如果这个AssetBundle在异步加载中，则直接堵塞主线程，返回Request.assetBundle
            for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.CurrAssetBundleInfo.AssetBundleName == assetbundlePath)
                {
                    return curr.Value.CurrAssetBundleCreateRequest.assetBundle;
                }
            }

            //加载资源包
            AssetBundle assetbundle = AssetBundleLoaderRoutine.LoadAssetBundle(assetbundlePath);

            //资源包注册到资源池
            AssetBundleReferenceEntity.Create(assetbundlePath, assetbundle);
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
                AssetLoaderRoutine routine = AssetLoaderRoutine.Create();

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
                };
                //加载资源
                routine.LoadAssetAsync(assetName, assetBundle);
            });
            AssetTaskGroup.Run();
        }
        public UniTask<Object> LoadAssetAsync(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null)
        {
            var task = new UniTaskCompletionSource<Object>();
            LoadAssetAction(assetName, assetBundle, onUpdate, (obj) =>
            {
                task.TrySetResult(obj);
            });
            return task.Task;
        }

        public Object LoadAsset(string assetName, AssetBundle assetBundle)
        {
            //如果这个Asset在异步加载中，则直接堵塞主线程，返回Request.asset
            for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.CurrAssetName == assetName)
                {
                    return curr.Value.CurrAssetBundleRequest.asset;
                }
            }

            return assetBundle.LoadAsset(assetName);
        }
        #endregion

        #region LoadMainAsset 加载主资源(自动加载依赖)
        /// <summary>
        /// 异步加载主资源(自动加载依赖)
        /// </summary>
        public async UniTask<T> LoadMainAssetAsync<T>(string assetPath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null) where T : Object
        {
            AssetReferenceEntity resEntity = await LoadMainAssetAsync(assetPath, onUpdate, onDownloadUpdate);
            return resEntity.Target as T;
        }
        public async UniTask<AssetReferenceEntity> LoadMainAssetAsync(string assetPath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            //从分类资源池(AssetPool)中查找主资源
            AssetReferenceEntity referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetPath);
            if (referenceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return referenceEntity;
            }

#if EDITORLOAD && UNITY_EDITOR
            referenceEntity = AssetReferenceEntity.Create(assetPath, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath));
#else
            //加载主资源包和依赖资源包
            AssetBundle mainAssetBundle = await GameEntry.Loader.LoadAssetBundleMainAndDependAsync(assetPath, onUpdate, onDownloadUpdate);

            //从分类资源池(AssetPool)中查找主资源 (再次查找是为了防止高并发情况下，异步加载AssetBundle后，AssetPool内已经有Asset对象了)
            referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetPath);
            if (referenceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return referenceEntity;
            }

            Object obj = await GameEntry.Loader.LoadAssetAsync(assetPath, mainAssetBundle);
            referenceEntity = AssetReferenceEntity.Create(assetPath, obj);
#endif

            return referenceEntity;
        }

        /// <summary>
        /// 同步加载主资源(自动加载依赖)
        /// </summary>
        public T LoadMainAsset<T>(string assetPath) where T : Object
        {
            return LoadMainAsset(assetPath).Target as T;
        }
        public AssetReferenceEntity LoadMainAsset(string assetPath)
        {
            //从分类资源池(AssetPool)中查找主资源
            AssetReferenceEntity referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetPath);
            if (referenceEntity != null)
            {
                //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                return referenceEntity;
            }

#if EDITORLOAD && UNITY_EDITOR
            referenceEntity = AssetReferenceEntity.Create(assetPath, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath));
#else
            //加载主资源包和依赖资源包
            AssetBundle mainAssetBundle = GameEntry.Loader.LoadAssetBundleMainAndDepend(assetPath);

            //加载主资源
            Object obj = GameEntry.Loader.LoadAsset(assetPath, mainAssetBundle);
            referenceEntity = AssetReferenceEntity.Create(assetPath, obj);
#endif

            if (referenceEntity.Target == null) GameEntry.LogError(LogCategory.Loader, "资源加载失败==" + assetPath);
            return referenceEntity;
        }
        #endregion
    }
}
using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYou
{
    public class ILRuntimeManager : ManagerBase
    {
        //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
        //大家在正式项目中请全局只创建一个AppDomain
        public static AppDomain m_AppDomain;

        internal override void Init()
        {
            GameEntry.Instance.StartCoroutine(LoadHotFixAssembly());
        }


        IEnumerator LoadHotFixAssembly()
        {
            //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
            m_AppDomain = new AppDomain();
#if EditorLoad && UNITY_EDITOR
			string dllPath = string.Format("file:///{0}/../Hotfix/bin/Debug/", GameEntry.Resource.LocalFilePath);
			//Debug.LogError("dllPath==" + dllPath);

			WWW www = new WWW(dllPath + "HotFix.dll");
			while (!www.isDone) yield return null;

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError(www.error);
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.LoadILRuntimeComplete);
				yield return null;
			}

			byte[] dll = www.bytes;
			www.Dispose();

			//PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
			www = new WWW(dllPath + "HotFix.pdb");
			while (!www.isDone) yield return null;

			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError(www.error);
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.LoadILRuntimeComplete);
				yield return null;
			}
			byte[] pdb = www.bytes;
			using (MemoryStream fs = new MemoryStream(dll))
			{
				using (MemoryStream p = new MemoryStream(pdb))
				{
					m_AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
				}
			}

			InitializeILRuntime();
			OnHotFixLoaded();
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.ILRuntimeAssetBundlePath, onComplete: (ResourceEntity bundleEntity) =>
            {
                AssetBundle bundle = bundleEntity.Target as AssetBundle;
                byte[] buffer = bundle.LoadAsset<TextAsset>("HotFix.dll.bytes").bytes;
                byte[] pdbbuffer = bundle.LoadAsset<TextAsset>("HotFix.pdb.bytes").bytes;

                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    m_AppDomain.LoadAssembly(fs);
                    //using (MemoryStream p = new MemoryStream(pdbbuffer))
                    //{
                    //    m_AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
                    //}
                }
                InitializeILRuntime();
                OnHotFixLoaded();
            });
#endif
            yield return null;
        }

        void InitializeILRuntime()
        {
            ////默认委托注册仅仅支持系统自带的Action以及Function
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<object>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<byte[]>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<string, Object, object, object, object>();
            //m_AppDomain.DelegateManager.RegisterMethodDelegate<int, GameObject>();


            //m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();


            ////自定义委托或Unity委托注册
            //m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
            //{
            //	return new UnityEngine.Events.UnityAction<bool>((a) =>
            //	{
            //		((System.Action<bool>)action)(a);
            //	});
            //});
            //m_AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
            //{
            //	return new UnityEngine.Events.UnityAction(() =>
            //	{
            //		((System.Action)action)();
            //	});
            //});
            //m_AppDomain.DelegateManager.RegisterDelegateConvertor<SocketEvent.OnActionHandler>((act) =>
            //{
            //	return new YouYou.SocketEvent.OnActionHandler((buffer) =>
            //	{
            //		((System.Action<byte[]>)act)(buffer);
            //	});
            //});
            //m_AppDomain.DelegateManager.RegisterDelegateConvertor<UIMultiScroller.OnItemCreateHandler>((act) =>
            //{
            //	return new UIMultiScroller.OnItemCreateHandler((index, obj) =>
            //	{
            //		((System.Action<int, GameObject>)act)(index, obj);
            //	});
            //});


        }

        void OnHotFixLoaded()
        {
            GameEntry.Event.CommonEvent.Dispatch(CommonEventId.LoadILRuntimeComplete);
            //含参调用静态方法
            m_AppDomain.Invoke("Hotfix.InstanceClass", "StaticFunTest", null, "kakakakak");

            //含参调用普通方法
            //object obj = m_AppDomain.Instantiate("Hotfix.InstanceClass");
            //m_AppDomain.Invoke("Hotfix.InstanceClass", "StaticFunTest", obj, "kakakakak");

            //实例化热更工程里的类
            //第一种实例化(可以带参数)
            //object obj = m_AppDomain.Instantiate("Hotfix.StaticFunTest");
            //第二种实例化（不带参数）
            //object obj = ((ILType)type).Instantiate();
            //int id = (int)m_AppDomain.Invoke("Hotfix.TestClass", "get_ID", obj, null);
            //Debug.Log("TestClass 中 ID:" + id);
        }
    }
}
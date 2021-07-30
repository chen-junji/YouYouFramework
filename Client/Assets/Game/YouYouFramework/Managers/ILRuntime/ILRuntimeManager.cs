using ILRuntime.Runtime.Enviorment;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYou
{
    public class ILRuntimeManager
    {
        //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
        //大家在正式项目中请全局只创建一个AppDomain
        public AppDomain AppDomain;

        public System.Action OnLoadDataTableComplete;

        internal void Init()
        {
            GameEntry.Instance.StartCoroutine(LoadHotFixAssembly());
        }


        IEnumerator LoadHotFixAssembly()
        {
            //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
            AppDomain = new AppDomain();
#if EDITORLOAD && UNITY_EDITOR
            string dllPath = string.Format("file:///{0}/Game/HotFix_Project~/bin/Debug/", GameEntry.Resource.LocalFilePath);
            //Debug.LogError("dllPath==" + dllPath);

            WWW www = new WWW(dllPath + "HotFix.dll");
            while (!www.isDone) yield return null;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                OnLoadDataTableComplete?.Invoke();
                yield return null;
            }

            byte[] dll = www.bytes;
            www.Dispose();

            //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
            //www = new WWW(dllPath + "HotFix.pdb");
            //while (!www.isDone) yield return null;

            //if (!string.IsNullOrEmpty(www.error))
            //{
            //	Debug.LogError(www.error);
            //	OnLoadDataTableComplete?.Invoke();
            //	yield return null;
            //}
            //byte[] pdb = www.bytes;

            MemoryStream fs = new MemoryStream(dll);
            AppDomain.LoadAssembly(fs);
            //MemoryStream p = new MemoryStream(pdb);
            //AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());

            InitializeILRuntime();
            OnHotFixLoaded();
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.ILRuntimeAssetBundlePath, onComplete: (AssetBundle bundle) =>
            {
                byte[] buffer = bundle.LoadAsset<TextAsset>("HotFix.dll.bytes").bytes;
                byte[] pdbbuffer = bundle.LoadAsset<TextAsset>("HotFix.pdb.bytes").bytes;

                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    AppDomain.LoadAssembly(fs);
                    //using (MemoryStream p = new MemoryStream(pdbbuffer))
                    //{
                    //    AppDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
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
            AppDomain.DelegateManager.RegisterMethodDelegate<bool>();
            AppDomain.DelegateManager.RegisterMethodDelegate<int>();
            AppDomain.DelegateManager.RegisterMethodDelegate<string>();
            AppDomain.DelegateManager.RegisterMethodDelegate<object>();
            AppDomain.DelegateManager.RegisterMethodDelegate<byte[]>();
            AppDomain.DelegateManager.RegisterMethodDelegate<string, Object, object, object, object>();
            AppDomain.DelegateManager.RegisterMethodDelegate<int, GameObject>();
            AppDomain.DelegateManager.RegisterMethodDelegate<UIFormBase>();
            AppDomain.DelegateManager.RegisterMethodDelegate<TaskRoutine>();
            AppDomain.DelegateManager.RegisterMethodDelegate<ILRuntimeForm>();

            //自定义委托或Unity委托注册
            AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<bool>>((action) =>
            {
                return new UnityEngine.Events.UnityAction<bool>((a) =>
                {
                    ((System.Action<bool>)action)(a);
                });
            });
            AppDomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((action) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((System.Action)action)();
                });
            });
            AppDomain.DelegateManager.RegisterDelegateConvertor<SocketEvent.OnActionHandler>((act) =>
            {
                return new SocketEvent.OnActionHandler((buffer) =>
                {
                    ((System.Action<byte[]>)act)(buffer);
                });
            });
            AppDomain.DelegateManager.RegisterDelegateConvertor<CommonEvent.OnActionHandler>((act) =>
            {
                return new CommonEvent.OnActionHandler((userData) =>
                {
                    ((System.Action<object>)act)(userData);
                });
            });
        }

        void OnHotFixLoaded()
        {
            AppDomain.Instantiate("Hotfix.GameEntryIL");

            //含参调用静态方法
            //AppDomain.Invoke("Hotfix.InstanceClass", "StaticFunTest", null, "kakakakak");

            //含参调用普通方法
            //object obj = AppDomain.Instantiate("Hotfix.InstanceClass");
            //AppDomain.Invoke("Hotfix.InstanceClass", "StaticFunTest", obj, "kakakakak");

            //实例化热更工程里的类
            //第一种实例化(可以带参数)
            // AppDomain.Instantiate("Hotfix.GameEntryIL");
            //第二种实例化（不带参数）
            //object obj = ((ILType)type).Instantiate();
            //int id = (int)AppDomain.Invoke("Hotfix.TestClass", "get_ID", obj, null);
            //Debug.Log("TestClass 中 ID:" + id);
        }
    }
}
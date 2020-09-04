using System;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Model;
using YouYouServer.Model.IHandler;

namespace YouYouServer.WebAccount
{
    /// <summary>
    /// 热更新管理器
    /// </summary>
    public sealed class HotFixMgr
    {
        /// <summary>
        /// 账号控制器处理器
        /// </summary>
        public static IAccountControllerHandler CurrAccountControllerHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// 加载可热更新的控制器
        /// </summary>
        public static void Load()
        {
            HotFixConfig.Load();
            HotFixHelper.LoadHotFixAssembly();

            //Activator.CreateInstance 根据类创建类实例
            CurrAccountControllerHandler = Activator.CreateInstance(HotFixHelper.HandlerTypeDic[ConstDefine.AccountControllerHandler]) as IAccountControllerHandler;
        }
    }
}

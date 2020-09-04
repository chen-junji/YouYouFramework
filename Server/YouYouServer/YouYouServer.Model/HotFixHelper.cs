using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Common;
using YouYouServer.Model.IHandler;
using YouYouServer.Model.ServerManager;

namespace YouYouServer.Model
{
    public sealed class HotFixHelper
    {
        public static Dictionary<string, Type> HandlerTypeDic = new Dictionary<string, Type>();

        public static Action OnLoadAssembly;

        /// <summary>
        /// 加载热更新程序集
        /// </summary>
        public static void LoadHotFixAssembly()
        {
            string assemblyName = HotFixConfig.GetParams("HotFixAssemblyName");

            Assembly assembly = Assembly.LoadFile(System.AppDomain.CurrentDomain.BaseDirectory + assemblyName);
            Type[] types = assembly.GetTypes();

            int len = types.Length;
            for (int i = 0; i < len; i++)
            {
                Type type = types[i];
                object[] objects = type.GetCustomAttributes(typeof(HandlerAttribute), true);
                if (objects.Length == 0)
                {
                    continue;
                }

                HandlerAttribute handlerAttribute = (HandlerAttribute)objects[0];
                HandlerTypeDic[handlerAttribute.TypeName] = type;
            }

            OnLoadAssembly?.Invoke();

            Console.WriteLine("LoadHotFixAssembly Success");
        }
    }
}

using System;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Logger;
using YouYouServer.Model;
using YouYouServer.Model.ServerManager;

namespace YouYouServer.GatewayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GatewayServer!");

            HotFixConfig.Load();
            HotFixHelper.LoadHotFixAssembly();
            ServerConfig.Init();
            DataTableManager.Init();
            LoggerMgr.Init();
            YFRedisClient.InitRedisClient();
            GatewayServerManager.Init();

            Console.ReadLine();
        }
    }
}
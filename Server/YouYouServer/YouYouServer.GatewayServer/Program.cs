using System;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Model;

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
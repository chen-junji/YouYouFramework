using System;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Model;

namespace YouYouServer.GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello GameServer!");

            HotFixConfig.Load();
            HotFixHelper.LoadHotFixAssembly();
            ServerConfig.Init();
            DataTableManager.Init();
            LoggerMgr.Init();
            YFRedisClient.InitRedisClient();

            GameServerManager.Init();

            Console.ReadLine();
        }
    }
}

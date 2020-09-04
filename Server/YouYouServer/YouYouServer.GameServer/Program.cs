using System;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Logger;
using YouYouServer.Model;
using YouYouServer.Model.ServerManager;

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

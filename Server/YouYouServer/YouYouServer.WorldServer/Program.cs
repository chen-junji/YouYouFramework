
using System;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Logger;
using YouYouServer.Model;
using YouYouServer.Model.ServerManager;

namespace YouYouServer.WorldServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello WorldServer!");

            HotFixConfig.Load();
            HotFixHelper.LoadHotFixAssembly();
            ServerConfig.Init();
            DataTableManager.Init();
            LoggerMgr.Init();
            YFRedisClient.InitRedisClient();
            WorldServerManager.Init();

            while (true)
            {
                string key = Console.ReadLine();
                if (key == "R")
                {
                    HotFixConfig.Load();
                    HotFixHelper.LoadHotFixAssembly();
                }
            }
        }
    }
}

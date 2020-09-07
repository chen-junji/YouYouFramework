
using System;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Model;

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

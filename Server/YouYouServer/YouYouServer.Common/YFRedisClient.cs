using CSRedis;
using System;
using YouYouServer.Common.Managers;

namespace YouYouServer.Common
{
    /// <summary>
    /// 悠游RedisClient
    /// </summary>
    public static class YFRedisClient
    {
        private static object lock_obj = new object();

        private static CSRedisClient m_CurrClient = null;

        /// <summary>
        /// InitRedisClient
        /// </summary>
        public static void InitRedisClient()
        {
            if (m_CurrClient == null)
            {
                lock (lock_obj)
                {
                    if (m_CurrClient == null)
                    {
                        m_CurrClient = new CSRedisClient(ServerConfig.RedisConnectionString);
                        RedisHelper.Initialization(m_CurrClient);

                        Console.WriteLine("RedisHelper Init Complete");
                    }
                }
            }
        }
    }
}
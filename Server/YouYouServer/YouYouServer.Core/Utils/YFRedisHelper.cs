using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YouYouServer.Core.Utils
{
    public class YFRedisHelper : RedisHelper
    {
        #region 悠游哈希CacheShell
        /// <summary>
        /// 悠游哈希CacheShell
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public static T YFCacheShell<T>(string key, string field, Func<string, T> getData)
        {
            T t = HGet<T>(key, field);
            if (t == null)
            {
                t = getData(field);
                HSet(key, field, t);
            }
            return t;
        }

        /// <summary>
        /// 悠游哈希CacheShellAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public static async Task<T> YFCacheShellAsync<T>(string key, string field, Func<string, Task<T>> getDataAsync)
        {
            T t = await HGetAsync<T>(key, field);
            if (t == null)
            {
                t = await getDataAsync(field);
                await HSetAsync(key, field, t);
            }
            return t;
        }
        #endregion
    }
}

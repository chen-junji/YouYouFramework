using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Core.Utils
{
    public class YFDateTimeUtil
    {
        #region GetTimestamp 获取时间戳
        /// <summary>
        /// 获取时间戳 定义为从格林尼治时间1970年01月01日00时00分00秒起至现在的总秒数
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            return (DateTime.UtcNow.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static long GetTimestamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        #endregion
    }
}

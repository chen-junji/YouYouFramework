using YouYouServer.Core;

namespace YouYouServer.Core
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LoggerEntity : YFMongoEntityBase
    {
        /// <summary>
        /// 日志等级
        /// </summary>
        public LoggerLevel Level;

        /// <summary>
        /// 日志分类
        /// </summary>
        public int Category;

        /// <summary>
        /// 日志内容
        /// </summary>
        public string Message;
    }
}
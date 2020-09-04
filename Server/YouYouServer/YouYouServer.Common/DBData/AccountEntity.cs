using YouYouServer.Core.YFMongoDB;

namespace YouYouServer.Common.DBData
{
    /// <summary>
    /// 账号实体
    /// </summary>
    public class AccountEntity : YFMongoEntityBase
    {
        /// <summary>
        /// 渠道号
        /// </summary>
        public short ChannelId;

        /// <summary>
        /// 设备唯一编号
        /// </summary>
        public string DeviceIdentifier;

        /// <summary>
        /// 设备型号
        /// </summary>
        public string DeviceModel;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password;
    }
}

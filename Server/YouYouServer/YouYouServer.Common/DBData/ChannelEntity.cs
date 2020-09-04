namespace YouYouServer.Common.DBData
{
    /// <summary>
    /// 渠道实体
    /// </summary>
    public class ChannelEntity
    {
        /// <summary>
        /// 渠道号
        /// </summary>
        public int ChannelId;

        /// <summary>
        /// 内部版本号
        /// </summary>
        public int InnerVersion;

        /// <summary>
        /// 资源版本号
        /// </summary>
        public string SourceVersion;

        /// <summary>
        /// 资源地址
        /// </summary>
        public string SourceUrl;

        /// <summary>
        /// 充值地址
        /// </summary>
        public string RechargeUrl;

        /// <summary>
        /// 充值服务器编号
        /// </summary>
        public int PayServerNo;

        /// <summary>
        /// 统计平台编号
        /// </summary>
        public string TDAppId;

        /// <summary>
        /// 是否开启统计
        /// </summary>
        public bool IsOpenTD;
    }
}
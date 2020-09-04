namespace YouYouServer.Common
{
    public sealed class ConstDefine
    {
        #region CollectionType 集合类型
        /// <summary>
        /// 集合类型
        /// </summary>
        public enum CollectionType
        {
            /// <summary>
            /// 日志
            /// </summary>
            Logger = 0,

            /// <summary>
            /// 角色
            /// </summary>
            Role = 10
        }
        #endregion

        #region ServerType 服务器类型
        /// <summary>
        /// 服务器类型
        /// </summary>
        public enum ServerType
        {
            /// <summary>
            /// 未设置
            /// </summary>
            None = -1,
            /// <summary>
            /// 中心服务器
            /// </summary>
            WorldServer = 0,

            /// <summary>
            /// 游戏服务器
            /// </summary>
            GameServer = 1,

            /// <summary>
            /// 网关服务器
            /// </summary>
            GatewayServer = 2
        }
        #endregion

        /// <summary>
        /// 网关服务器状态
        /// </summary>
        public enum GatewayServerStatus
        {
            /// <summary>
            /// 未设置
            /// </summary>
            None = -1,
            /// <summary>
            /// 注册到游戏服完毕
            /// </summary>
            RegGameServerSuccess = 0
        }

        public const string AccountControllerHandler = "AccountControllerHandler";
        public const string PlayerForWorldClientHandler = "PlayerForWorldClientHandler";
    }
}
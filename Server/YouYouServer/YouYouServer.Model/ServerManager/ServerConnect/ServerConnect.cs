using System;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Common;
using YouYouServer.Core.Logger;

namespace YouYouServer.Model.ServerManager
{
    /// <summary>
    /// 服务器连接器
    /// </summary>
    public class ServerConnect
    {
        /// <summary>
        /// Socket事件监听派发器
        /// </summary>
        public EventDispatcher EventDispatcher
        {
            get;
            private set;
        }

        /// <summary>
        /// Socket连接器
        /// </summary>
        public ClientSocket ClientSocket
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前的服务器配置
        /// </summary>
        private ServerConfig.Server m_CurrConfig;

        /// <summary>
        /// 处理中转协议
        /// </summary>
        public BaseAction<ushort, ProtoCategory, byte[]> OnCarryProto;

        public ServerConnect(ServerConfig.Server serverConfig)
        {
            m_CurrConfig = serverConfig;

            EventDispatcher = new EventDispatcher();
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect(Action onConnectSuccess = null, Action onConnectFail = null)
        {
            ClientSocket = new ClientSocket(EventDispatcher);
            ClientSocket.OnConnectSuccess = () =>
            {
                LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "Connect Server Success");
                onConnectSuccess?.Invoke();
            };

            ClientSocket.OnConnectFail = () =>
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "Connect Server Fail");
                onConnectFail?.Invoke();
            };

            //处理服务器返回的中转协议
            ClientSocket.OnCarryProto = (ushort protoCode, ProtoCategory protoCategory, byte[] buffer) =>
            {
                OnCarryProto?.Invoke(protoCode, protoCategory, buffer);
            };
            ClientSocket.Connect(m_CurrConfig.Ip, m_CurrConfig.Port);
        }
    }
}

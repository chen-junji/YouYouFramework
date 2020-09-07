using System;
using System.Net.Sockets;
using YouYou.Proto;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 服务器客户端
    /// </summary>
    public class ServerClient : IDisposable
    {
        /// <summary>
        /// 服务器编号
        /// </summary>
        public int ServerId
        {
            get;
            private set;
        }

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
        /// 断开连接
        /// </summary>
        public Action OnDisConnect;

        /// <summary>
        /// 处理中转协议
        /// </summary>
        public BaseAction<ushort, ProtoCategory, byte[]> OnCarryProto;

        public ServerClient(Socket socket)
        {
            EventDispatcher = new EventDispatcher();


            ClientSocket = new ClientSocket(socket, EventDispatcher);
            ClientSocket.OnDisConnect = () => { OnDisConnect?.Invoke(); };
            ClientSocket.OnCarryProto = (ushort protoCode, ProtoCategory protoCategory, byte[] buffer) =>
            {
                OnCarryProto?.Invoke(protoCode, protoCategory, buffer);
            };
            AddEventListener();
        }

        /// <summary>
        /// 监听服务器客户端 连接到服务器
        /// </summary>
        private void AddEventListener()
        {
            EventDispatcher.AddEventListener(ProtoIdDefine.Proto_GS2WS_RegGameServer, OnGS2WS_RegGameServer);
            EventDispatcher.AddEventListener(ProtoIdDefine.Proto_GWS2WS_RegGatewayServer, OnGWS2WS_RegGatewayServer);
            EventDispatcher.AddEventListener(ProtoIdDefine.Proto_GWS2GS_RegGatewayServer, OnGWS2GS_RegGatewayServer);
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        private void RemoveEventListener()
        {
            EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_GS2WS_RegGameServer, OnGS2WS_RegGameServer);
            EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_GWS2WS_RegGatewayServer, OnGWS2WS_RegGatewayServer);
            EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_GWS2GS_RegGatewayServer, OnGWS2GS_RegGatewayServer);
        }

        #region OnGS2WS_RegGameServer 游戏服务器注册到中心服务器
        /// <summary>
        /// 游戏服务器注册到中心服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void OnGS2WS_RegGameServer(byte[] buffer)
        {
            GS2WS_RegGameServer proto = GS2WS_RegGameServer.Parser.ParseFrom(buffer);
            ServerConfig.Server server = ServerConfig.GetServer(ConstDefine.ServerType.GameServer, proto.ServerId);
            if (server != null)
            {
                ServerId = proto.ServerId;
                WorldServerManager.RegisterGameServerClient(new GameServerClient(this));
            }
            else
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "RegGameServer Fail ServerId={0}", proto.ServerId);
            }
        }
        #endregion

        #region OnGWS2WS_RegGatewayServer 网关服务器注册到中心服务器
        /// <summary>
        /// 网关服务器注册到中心服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void OnGWS2WS_RegGatewayServer(byte[] buffer)
        {
            GWS2WS_RegGatewayServer proto = GWS2WS_RegGatewayServer.Parser.ParseFrom(buffer);
            ServerConfig.Server server = ServerConfig.GetServer(ConstDefine.ServerType.GatewayServer, proto.ServerId);
            if (server != null)
            {
                ServerId = proto.ServerId;
                WorldServerManager.RegisterGatewayServerClient(new GatewayServerForWorldClient(this));
            }
            else
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "RegGameServer Fail ServerId={0}", proto.ServerId);
            }
        }
        #endregion

        #region OnGWS2GS_RegGatewayServer 网关服务器注册到游戏服务器
        /// <summary>
        /// 网关服务器注册到游戏服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void OnGWS2GS_RegGatewayServer(byte[] buffer)
        {
            GWS2GS_RegGatewayServer proto = GWS2GS_RegGatewayServer.Parser.ParseFrom(buffer);
            ServerConfig.Server server = ServerConfig.GetServer(ConstDefine.ServerType.GatewayServer, proto.ServerId);
            if (server != null)
            {
                ServerId = proto.ServerId;
                GameServerManager.RegisterGatewayServerClient(new GatewayServerForGameClient(this));
            }
            else
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "RegGameServer Fail ServerId={0}", proto.ServerId);
            }
        }
        #endregion

        public void Dispose()
        {
            RemoveEventListener();
        }
    }
}

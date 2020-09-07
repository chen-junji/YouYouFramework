using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 游戏服管理器
    /// </summary>
    public sealed class GameServerManager
    {
        /// <summary>
        /// 网关服务器客户端
        /// </summary>
        private static Dictionary<int, GatewayServerForGameClient> m_GatewayServerClientDic;

        /// <summary>
        /// 游戏服务器上的玩家字典
        /// </summary>
        private static Dictionary<long, PlayerForGameClient> m_PlayerForGameClient;

        /// <summary>
        /// 当前服务器
        /// </summary>
        public static ServerConfig.Server CurrServer;

        /// <summary>
        /// 连接到中心服务器代理
        /// </summary>
        public static GameConnectWorldAgent ConnectWorldAgent;


        /// <summary>
        /// Socket监听
        /// </summary>
        private static Socket m_ListenSocket;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            m_GatewayServerClientDic = new Dictionary<int, GatewayServerForGameClient>();
            m_PlayerForGameClient = new Dictionary<long, PlayerForGameClient>();

            CurrServer = ServerConfig.GetCurrServer();

            //实例化连接到中心服务器代理
            ConnectWorldAgent = new GameConnectWorldAgent();
            ConnectWorldAgent.RegisterToWorldServer();

            StarListen();
        }

        #region StarListen 启动监听
        /// <summary>
        /// 启动监听
        /// </summary>
        private static void StarListen()
        {
            //实例化socket
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //向操作系统申请一个可用的ip和端口用来通讯
            m_ListenSocket.Bind(new IPEndPoint(IPAddress.Parse(CurrServer.Ip), CurrServer.Port));

            m_ListenSocket.Listen(20);

            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "启动监听{0}成功", m_ListenSocket.LocalEndPoint.ToString());

            Thread mThread = new Thread(ListenClientCallBack);
            mThread.Start();
        }
        #endregion;

        #region ListenClientCallBack 监听回调
        /// <summary>
        /// 监听回调
        /// </summary>
        /// <param name="obj"></param>
        private static void ListenClientCallBack(object obj)
        {
            while (true)
            {
                //接收服务器客户端请求
                Socket socket = m_ListenSocket.Accept();

                IPEndPoint point = (IPEndPoint)socket.RemoteEndPoint;

                LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "客户端IP={0} Port={1}已经连接", point.Address.ToString(), point.Port);

                //实例化一个服务器客户端
                new ServerClient(socket);
            }
        }
        #endregion;

        #region RegisterGatewayServerClient 注册网关服务器客户端
        /// <summary>
        /// 注册网关服务器客户端
        /// </summary>
        /// <param name="gatewayServerClient"></param>
        public static void RegisterGatewayServerClient(GatewayServerForGameClient gatewayServerForGameClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RegisterGatewayServerClient Success ServerId={0}", gatewayServerForGameClient.ServerId);
            m_GatewayServerClientDic.Add(gatewayServerForGameClient.ServerId, gatewayServerForGameClient);
        }
        #endregion

        #region RemoveGatewayServerClient 移除网关服务器客户端
        /// <summary>
        /// 移除网关服务器客户端
        /// </summary>
        /// <param name="gatewayServerForGameClient"></param>
        public static void RemoveGatewayServerClient(GatewayServerForGameClient gatewayServerForGameClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RemoveGatewayServerClient Success ServerId={0}", gatewayServerForGameClient.ServerId);
            m_GatewayServerClientDic.Remove(gatewayServerForGameClient.ServerId);
        }
        #endregion

        #region RegisterGatewayServerClient 注册游戏服务器上的玩家客户端
        /// <summary>
        /// 注册游戏服务器上的玩家客户端
        /// </summary>
        /// <param name="gatewayServerClient"></param>
        public static void RegisterPlayerForGameClient(PlayerForGameClient playerForGameClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RegisterPlayerForGameClient Success AccountId={0}", playerForGameClient.AccountId);
            m_PlayerForGameClient.Add(playerForGameClient.AccountId, playerForGameClient);
        }
        #endregion

        #region GetPlayerClient 获取游戏服务器上的玩家客户端
        /// <summary>
        /// 获取游戏服务器上的玩家客户端
        /// </summary>
        /// <param name="accountId">玩家账号</param>
        /// <returns></returns>
        public static PlayerForGameClient GetPlayerClient(long accountId)
        {
            PlayerForGameClient playerForGameClient = null;
            m_PlayerForGameClient.TryGetValue(accountId, out playerForGameClient);
            return playerForGameClient;
        }
        #endregion

        #region RemovePlayerForGameClient 移除游戏服务器上的玩家客户端
        /// <summary>
        /// 移除游戏服务器上的玩家客户端
        /// </summary>
        /// <param name="gameServerClient"></param>
        public static void RemovePlayerForGameClient(PlayerForGameClient playerForGameClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RemovePlayerForGameClient Success AccountId={0}", playerForGameClient.AccountId);
            m_PlayerForGameClient.Remove(playerForGameClient.AccountId);
        }
        #endregion
    }
}
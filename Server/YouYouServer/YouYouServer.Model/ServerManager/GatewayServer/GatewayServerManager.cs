using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using YouYouServer.Common;
using YouYouServer.Common.Managers;
using YouYouServer.Core.Logger;

namespace YouYouServer.Model.ServerManager
{
    /// <summary>
    /// 网关服务器管理器
    /// </summary>
    public sealed class GatewayServerManager
    {
        /// <summary>
        /// 玩家客户端
        /// </summary>
        private static Dictionary<long, PlayerForGatewayClient> m_PlayerClientDic;

        /// <summary>
        /// 当前服务器
        /// </summary>
        public static ServerConfig.Server CurrServer;

        /// <summary>
        /// 游戏服配置列表
        /// </summary>
        private static List<ServerConfig.Server> LstGameServer = null;

        /// <summary>
        /// 网关连接到游戏服的代理
        /// </summary>
        private static Dictionary<int, GatewayConnectGameAgent> m_GatewayConnectGameAgentDic;

        /// <summary>
        /// 连接到中心服务器代理
        /// </summary>
        public static GatewayConnectWorldAgent ConnectWorldAgent;

        /// <summary>
        /// Socket监听
        /// </summary>
        private static Socket m_ListenSocket;

        public static void Init()
        {
            m_PlayerClientDic = new Dictionary<long, PlayerForGatewayClient>();
            m_GatewayConnectGameAgentDic = new Dictionary<int, GatewayConnectGameAgent>();

            CurrServer = ServerConfig.GetCurrServer();
            LstGameServer = ServerConfig.GetServerByType(ConstDefine.ServerType.GameServer);

            //实例化连接到中心服务器代理
            ConnectWorldAgent = new GatewayConnectWorldAgent();
            ConnectWorldAgent.RegisterToWorldServer();

            StarListen();
        }

        #region ToRegGameServer 注册到游戏服
        /// <summary>
        /// 注册到游戏服
        /// </summary>
        public static void ToRegGameServer()
        {
            //拿到要目标游戏服列表
            int len = LstGameServer.Count;
            for (int i = 0; i < len; i++)
            {
                GatewayConnectGameAgent agent = new GatewayConnectGameAgent(LstGameServer[i]);
                agent.RegisterToGameServer();

                //把这个连接到游戏服代理加入字典
                m_GatewayConnectGameAgentDic[agent.TargetServerConfig.ServerId] = agent;
            }

            //通知中心服务器 注册游戏服完毕
            ConnectWorldAgent.ToRegGameServerSuccess();
        }
        #endregion

        /// <summary>
        /// 获取游戏服的代理
        /// </summary>
        /// <param name="gameServerId"></param>
        /// <returns></returns>
        public static GatewayConnectGameAgent GetGameServerAgent(int gameServerId)
        {
            GatewayConnectGameAgent agent = null;
            m_GatewayConnectGameAgentDic.TryGetValue(gameServerId, out agent);
            return agent;
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

            m_ListenSocket.Listen(3000);

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
                new PlayerForGatewayClient(socket);
            }
        }
        #endregion;

        /// <summary>
        /// 注册玩家客户端
        /// </summary>
        /// <param name="playerClient"></param>
        public static void RegisterPlayerClient(PlayerForGatewayClient playerClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RegisterPlayerClient Success AccountId={0}", playerClient.AccountId);
            m_PlayerClientDic.Add(playerClient.AccountId, playerClient);
        }

        /// <summary>
        /// 获取网关服务器上的玩家客户端
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static PlayerForGatewayClient GetPlayerClient(long accountId)
        {
            PlayerForGatewayClient playerForGatewayClient = null;
            m_PlayerClientDic.TryGetValue(accountId, out playerForGatewayClient);
            return playerForGatewayClient;
        }

        /// <summary>
        /// 移除玩家客户端
        /// </summary>
        /// <param name="playerClient"></param>
        public static void RemovePlayerClient(PlayerForGatewayClient playerClient)
        {
            LoggerMgr.Log(Core.LoggerLevel.Log, LogType.SysLog, "RemovePlayerClient Success AccountId={0}", playerClient.AccountId);
            m_PlayerClientDic.Remove(playerClient.AccountId);
        }
    }
}
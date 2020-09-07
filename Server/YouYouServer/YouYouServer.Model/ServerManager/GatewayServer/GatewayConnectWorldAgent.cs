using System.Collections.Generic;
using YouYou.Proto;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 网关服务器连接中心服务器代理
    /// </summary>
    public class GatewayConnectWorldAgent : ConnectAgentBase
    {
        public GatewayConnectWorldAgent()
        {
            List<ServerConfig.Server> servers = ServerConfig.GetServerByType(ConstDefine.ServerType.WorldServer);
            if (servers != null && servers.Count == 1)
            {
                TargetServerConfig = servers[0];
                TargetServerConnect = new ServerConnect(TargetServerConfig);
                TargetServerConnect.OnCarryProto = OnCarryProto;
                AddEventListener();
            }
            else
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "No WorldServer");
            }
        }

        public override void AddEventListener()
        {
            base.AddEventListener();

            TargetServerConnect.EventDispatcher.AddEventListener(ProtoIdDefine.Proto_WS2GWS_ToRegGameServer, OnWS2GWS_ToRegGameServer);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();

            TargetServerConnect.EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_WS2GWS_ToRegGameServer, OnWS2GWS_ToRegGameServer);
        }



        /// <summary>
        /// 收到中转协议并处理
        /// </summary>
        /// <param name="protoCode">协议编号</param>
        /// <param name="protoCategory">协议分类</param>
        /// <param name="buffer">协议内容</param>
        private void OnCarryProto(ushort protoCode, ProtoCategory protoCategory, byte[] buffer)
        {
            //网关服务器端收到的中转消息 都是经过中转的（中心服务器或者游戏服 发过来的）
            //所以这里直接解析中转协议
            CarryProto proto = CarryProto.GetProto(buffer);

            if (proto.CarryProtoCategory == ProtoCategory.WorldServer2Client)
            {
                long accountId = proto.AccountId;

                //1.找到在网关服务器上的玩家客户端
                PlayerForGatewayClient playerForGatewayClient = GatewayServerManager.GetPlayerClient(accountId);
                if (playerForGatewayClient != null)
                {
                    //2.给玩家发消息
                    playerForGatewayClient.ClientSocket.SendMsg(proto.CarryProtoId, (byte)proto.CarryProtoCategory, proto.Buffer);
                }
            }
        }

        #region RegisterToWorldServer 注册到中心服务器
        /// <summary>
        /// 注册到中心服务器
        /// </summary>
        public void RegisterToWorldServer()
        {
            //连接到中心服务器
            TargetServerConnect.Connect(onConnectSuccess: () =>
            {
                //告诉中心服务器 我是谁
                GWS2WS_RegGatewayServer proto = new GWS2WS_RegGatewayServer();
                proto.ServerId = GatewayServerManager.CurrServer.ServerId;
                TargetServerConnect.ClientSocket.SendMsg(proto);
            });
        }
        #endregion

        /// <summary>
        /// 收到中心服务器发的注册到游戏服消息
        /// </summary>
        /// <param name="buffer"></param>
        private void OnWS2GWS_ToRegGameServer(byte[] buffer)
        {
            GatewayServerManager.ToRegGameServer();
        }

        /// <summary>
        /// 通知中心服务器注册游戏服完毕
        /// </summary>
        public void ToRegGameServerSuccess()
        {
            GWS2WS_RegGameServerSuccess proto = new GWS2WS_RegGameServerSuccess();
            TargetServerConnect.ClientSocket.SendMsg(proto);
        }
    }
}
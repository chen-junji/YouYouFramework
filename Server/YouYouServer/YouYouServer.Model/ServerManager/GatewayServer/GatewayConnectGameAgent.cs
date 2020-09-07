using YouYou.Proto;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 网关服务器链接到游戏服务器代理
    /// </summary>
    public class GatewayConnectGameAgent : ConnectAgentBase
    {
        public GatewayConnectGameAgent(ServerConfig.Server server)
        {
            TargetServerConfig = server;
            if (TargetServerConfig != null)
            {
                //连接到游戏服务器
                TargetServerConnect = new ServerConnect(TargetServerConfig);
                TargetServerConnect.OnCarryProto = OnCarryProto;
                AddEventListener();
            }
            else
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, LogType.SysLog, "No GameServer");
            }
        }

        /// <summary>
        /// 收到中转协议并处理
        /// </summary>
        /// <param name="protoId">协议编号</param>
        /// <param name="protoCategory">协议分类</param>
        /// <param name="buffer">协议内容</param>
        private void OnCarryProto(ushort protoId, ProtoCategory protoCategory, byte[] buffer)
        {
            //网关服务器端收到的中转消息 都是经过中转的（中心服务器或者游戏服 发过来的）
            //所以这里直接解析中转协议
            CarryProto proto = CarryProto.GetProto(buffer);

            if (proto.CarryProtoCategory == ProtoCategory.GameServer2Client)
            {
                long accountId = proto.AccountId;

                //1.找到在网关服务器上的玩家客户端
                PlayerForGatewayClient playerForGatewayClient = GatewayServerManager.GetPlayerClient(accountId);
                if (playerForGatewayClient != null)
                {
                    //2.给玩家发消息
                    playerForGatewayClient.ClientSocket.SendMsg(proto);
                }
            }
        }

        public override void AddEventListener()
        {
            base.AddEventListener();
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
        }

        #region RegisterToGameServer 注册到游戏服务器
        /// <summary>
        /// 注册到游戏服务器
        /// </summary>
        public void RegisterToGameServer()
        {
            TargetServerConnect.Connect(onConnectSuccess: () =>
            {
                //告诉游戏服务器 我是谁
                GWS2GS_RegGatewayServer proto = new GWS2GS_RegGatewayServer();
                proto.ServerId = GatewayServerManager.CurrServer.ServerId;
                TargetServerConnect.ClientSocket.SendMsg(proto);
            });
        }
        #endregion
    }
}
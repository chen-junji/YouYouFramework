using System;
using System.Collections.Generic;
using System.Text;
using YouYou.Proto;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Core.Common;

namespace YouYouServer.Model.ServerManager
{
    /// <summary>
    /// 作为中心服务器的网关服务器客户端
    /// </summary>
    public class GatewayServerForWorldClient : ServerClientBase
    {
        /// <summary>
        /// 当前网关服务器客户端状态
        /// </summary>
        public ConstDefine.GatewayServerStatus CurrServerStatus
        {
            get;
            private set;
        }

        public GatewayServerForWorldClient(ServerClient serverClient) : base(serverClient)
        {
            CurrServerStatus = ConstDefine.GatewayServerStatus.None;

            CurrServerClient.OnDisConnect = () =>
            {
                Dispose();
                WorldServerManager.RemoveGatewayServerClient(this);
            };
            //处理中转协议
            CurrServerClient.OnCarryProto = OnCarryProto;

            AddEventListener();
        }

        public override void AddEventListener()
        {
            base.AddEventListener();

            CurrServerClient.EventDispatcher.AddEventListener(ProtoIdDefine.Proto_GWS2WS_RegGameServerSuccess, OnGWS2WS_RegGameServerSuccess);
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();

            CurrServerClient.EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_GWS2WS_RegGameServerSuccess, OnGWS2WS_RegGameServerSuccess);
        }

        /// <summary>
        /// 通知网关服务器去注册游戏服
        /// </summary>
        public void SendToRegGameServer()
        {
            WS2GWS_ToRegGameServer proto = new WS2GWS_ToRegGameServer();
            CurrServerClient.ClientSocket.SendMsg(proto);
        }

        /// <summary>
        /// 网关服务器通知中心服务器注册到游戏服完毕
        /// </summary>
        /// <param name="buffer"></param>
        private void OnGWS2WS_RegGameServerSuccess(byte[] buffer)
        {
            CurrServerStatus = ConstDefine.GatewayServerStatus.RegGameServerSuccess;

            WorldServerManager.CheckAllGatewayServerRegisterGameServerComplete();
        }

        /// <summary>
        /// 收到中转协议并处理
        /// </summary>
        /// <param name="protoCode">协议编号</param>
        /// <param name="protoCategory">协议分类</param>
        /// <param name="buffer">协议内容</param>
        private void OnCarryProto(ushort protoCode, ProtoCategory protoCategory, byte[] buffer)
        {
            //中心服务器端收到的中转消息 都是经过中转的
            //所以这里直接解析中转协议
            CarryProto proto = CarryProto.GetProto(buffer);

            if (proto.CarryProtoCategory == ProtoCategory.Client2WorldServer)
            {
                long accountId = proto.AccountId;

                //1.找到在中心服务器上的玩家客户端
                PlayerForWorldClient playerForWorldClient = WorldServerManager.GetPlayerClient(accountId);
                if (playerForWorldClient == null)
                {
                    //如果找不到 进行注册
                    playerForWorldClient = new PlayerForWorldClient(accountId, this);
                    WorldServerManager.RegisterPlayerForWorldClient(playerForWorldClient);
                }

                //2.给这个玩家客户端派发消息
                playerForWorldClient.EventDispatcher.Dispatch(proto.CarryProtoId, proto.Buffer);
            }
        }
    }
}

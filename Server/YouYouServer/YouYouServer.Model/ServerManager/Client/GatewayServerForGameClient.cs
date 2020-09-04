using System;
using System.Collections.Generic;
using System.Text;
using YouYouServer.Common;
using YouYouServer.Core;
using YouYouServer.Core.Common;

namespace YouYouServer.Model.ServerManager
{
    /// <summary>
    /// 作为游戏服务器的网关服务器客户端
    /// </summary>
    public class GatewayServerForGameClient : ServerClientBase
    {
        /// <summary>
        /// 当前网关服务器客户端状态
        /// </summary>
        public ConstDefine.GatewayServerStatus CurrServerStatus
        {
            get;
            private set;
        }

        public GatewayServerForGameClient(ServerClient serverClient) : base(serverClient)
        {
            CurrServerStatus = ConstDefine.GatewayServerStatus.None;

            CurrServerClient.OnDisConnect = () =>
            {
                Dispose();
                GameServerManager.RemoveGatewayServerClient(this);
            };
            //处理中转协议
            CurrServerClient.OnCarryProto = OnCarryProto;

            AddEventListener();
        }

        public override void AddEventListener()
        {
            base.AddEventListener();
        }

        public override void RemoveEventListener()
        {
            base.RemoveEventListener();
        }

        /// <summary>
        /// 收到中转协议并处理
        /// </summary>
        /// <param name="protoCode">协议编号</param>
        /// <param name="protoCategory">协议分类</param>
        /// <param name="buffer">协议内容</param>
        private void OnCarryProto(ushort protoCode, ProtoCategory protoCategory, byte[] buffer)
        {
            //游戏服务器端收到的中转消息 都是经过中转的
            //所以这里直接解析中转协议
            CarryProto proto = CarryProto.GetProto(buffer);

            if (proto.CarryProtoCategory == ProtoCategory.Client2GameServer)
            {
                long accountId = proto.AccountId;

                //1.找到在游戏服务器上的玩家客户端
                PlayerForGameClient playerForGameClient = GameServerManager.GetPlayerClient(accountId);
                if (playerForGameClient == null)
                {
                    //如果找不到 进行注册
                    playerForGameClient = new PlayerForGameClient(accountId, this);
                    GameServerManager.RegisterPlayerForGameClient(playerForGameClient);
                }

                //2.给这个玩家客户端派发消息
                playerForGameClient.EventDispatcher.Dispatch(proto.CarryProtoId, proto.Buffer);
            }
        }
    }
}

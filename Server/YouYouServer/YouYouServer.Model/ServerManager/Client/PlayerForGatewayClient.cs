using System.Net.Sockets;
using YouYou.Proto;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 玩家在网关服务器的客户端
    /// </summary>
    public class PlayerForGatewayClient : PlayerClientBase
    {
        /// <summary>
        /// 当前玩家在哪个游戏服
        /// </summary>
        public int CurrInGameServerId = 1;

        /// <summary>
        /// Socket连接器
        /// </summary>
        public ClientSocket ClientSocket
        {
            get;
            private set;
        }

        public PlayerForGatewayClient(Socket socket) : base()
        {
            ClientSocket = new ClientSocket(socket, EventDispatcher);
            ClientSocket.OnDisConnect = () =>
            {
                Dispose();
                GatewayServerManager.RemovePlayerClient(this);

                //通知中心服务器和游戏服 玩家下线了 todu
            };

            //处理中转协议
            ClientSocket.OnCarryProto = OnCarryProto;

            AddEventListener();
        }

        public void AddEventListener()
        {
            EventDispatcher.AddEventListener(ProtoIdDefine.Proto_C2GWS_RegClient, OnC2GWS_RegClient);
        }

        public void RemoveEventListener()
        {
            EventDispatcher.RemoveEventListener(ProtoIdDefine.Proto_C2GWS_RegClient, OnC2GWS_RegClient);
        }

        /// <summary>
        /// 注册客户端
        /// </summary>
        /// <param name="buffer"></param>
        private void OnC2GWS_RegClient(byte[] buffer)
        {
            C2GWS_RegClient proto = (C2GWS_RegClient)C2GWS_RegClient.Descriptor.Parser.ParseFrom(buffer);

            //此处可以加个验证 验证账号合法性

            AccountId = proto.AccountId;
            GatewayServerManager.RegisterPlayerClient(this);
            SendRegClientResult();
        }

        /// <summary>
        /// 向客户端发送注册结果
        /// </summary>
        private void SendRegClientResult()
        {
            GWS2C_ReturnRegClient proto = new GWS2C_ReturnRegClient();
            proto.Result = true;
            ClientSocket.SendMsg(proto);
        }

        /// <summary>
        /// 收到中转协议并处理
        /// </summary>
        /// <param name="protoCode">协议编号</param>
        /// <param name="protoCategory">协议分类</param>
        /// <param name="buffer">协议内容</param>
        private void OnCarryProto(ushort protoCode, ProtoCategory protoCategory, byte[] buffer)
        {
            switch (protoCategory)
            {
                case ProtoCategory.CarryProto:
                    break;
                case ProtoCategory.Client2GameServer:
                    break;
                case ProtoCategory.Client2WorldServer:
                    {
                        CarrySendToWorldServer(protoCode, buffer);
                    }
                    break;
            }
        }

        /// <summary>
        /// 中转发送到中心服的消息
        /// </summary>
        /// <param name="protoCode">协议编号</param>
        /// <param name="buffer">内容</param>
        private void CarrySendToWorldServer(ushort protoCode, byte[] buffer)
        {
            CarryProto proto = new CarryProto(AccountId, protoCode, ProtoCategory.Client2WorldServer, buffer);

            //通过中心服务器连接代理
            GatewayServerManager.ConnectWorldAgent.TargetServerConnect.ClientSocket.SendMsg(
                proto
                );
        }
    }
}
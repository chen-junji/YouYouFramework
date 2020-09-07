using Google.Protobuf;
using YouYou;
using YouYouServer.Common;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    /// <summary>
    /// 游戏服务器上的玩家客户端
    /// </summary>
    public class PlayerForGameClient : PlayerClientBase
    {
        /// <summary>
        /// 当前角色
        /// </summary>
        public RoleEntity CurrRole
        {
            get;
            private set;
        }

        /// <summary>
        /// 这个玩家所在的网关
        /// </summary>
        private GatewayServerForGameClient m_GatewayServerForGameClient;

        public PlayerForGameClient(long accountId, GatewayServerForGameClient gatewayServerForGameClient) : base()
        {
            AccountId = accountId;
            m_GatewayServerForGameClient = gatewayServerForGameClient;
        }

        /// <summary>
        /// 发送中转协议到客户端
        /// </summary>
        /// <param name="proto"></param>
        public void SendCarryToClient(IProto proto)
        {
            CarryProto carryProto = new CarryProto(AccountId, proto.ProtoId, proto.Category, proto.ToByteArray());
            m_GatewayServerForGameClient.CurrServerClient.ClientSocket.SendMsg(
                carryProto
                );
        }
    }
}
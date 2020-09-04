using Google.Protobuf;
using System;
using YouYou;
using YouYouServer.Common;
using YouYouServer.Common.DBData;
using YouYouServer.Core.Common;
using YouYouServer.Model.IHandler;

namespace YouYouServer.Model.ServerManager
{
    /// <summary>
    /// 中心服务器上的玩家客户端
    /// </summary>
    public class PlayerForWorldClient : PlayerClientBase
    {
        /// <summary>
        /// 当前角色
        /// </summary>
        public RoleEntity CurrRole
        {
            get;
            set;
        }

        /// <summary>
        /// 这个玩家所在的网关
        /// </summary>
        private GatewayServerForWorldClient m_GatewayServerForWorldClient;

        public PlayerForWorldClient(long accountId, GatewayServerForWorldClient gatewayServerForWorldClient) : base()
        {
            AccountId = accountId;
            m_GatewayServerForWorldClient = gatewayServerForWorldClient;

            HotFixHelper.OnLoadAssembly += InitPlayerForWorldClientHandler;
            InitPlayerForWorldClientHandler();
        }

        private IPlayerForWorldClientHandler m_CurrHandler;

        private void InitPlayerForWorldClientHandler()
        {
            if (m_CurrHandler != null)
            {
                //把旧的实例释放
                m_CurrHandler.Dispose();
                m_CurrHandler = null;
            }

            m_CurrHandler = Activator.CreateInstance(HotFixHelper.HandlerTypeDic[ConstDefine.PlayerForWorldClientHandler]) as IPlayerForWorldClientHandler;
            m_CurrHandler.Init(this);

            Console.WriteLine("InitPlayerForWorldClientHandler");
        }

        /// <summary>
        /// 发送中转协议到客户端
        /// </summary>
        /// <param name="proto"></param>
        public void SendCarryToClient(IProto proto)
        {
            CarryProto carryProto = new CarryProto(AccountId, proto.ProtoId, proto.Category, proto.ToByteArray());
            m_GatewayServerForWorldClient.CurrServerClient.ClientSocket.SendMsg(
                carryProto
                );
        }
    }
}

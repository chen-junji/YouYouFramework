using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Model.ServerManager
{
    //游戏服务器客户端
    public class GameServerClient : ServerClientBase
    {
        public GameServerClient(ServerClient serverClient) : base(serverClient)
        {
            //断开连接时
            CurrServerClient.OnDisConnect = () =>
            {
                Dispose();
                WorldServerManager.RemoveGameServerClient(this);
            };

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
    }
}
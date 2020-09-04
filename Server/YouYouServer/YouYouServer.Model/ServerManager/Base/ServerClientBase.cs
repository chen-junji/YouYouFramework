using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Model.ServerManager
{
    public abstract class ServerClientBase : IDisposable
    {
        /// <summary>
        /// 当前服务器客户端
        /// </summary>
        public ServerClient CurrServerClient
        {
            get;
            private set;
        }

        /// <summary>
        /// 服务器编号
        /// </summary>
        public int ServerId
        {
            get;
            private set;
        }

        public ServerClientBase(ServerClient serverClient)
        {
            CurrServerClient = serverClient;
            ServerId = CurrServerClient.ServerId;
        }

        // <summary>
        /// 监听
        /// </summary>
        public virtual void AddEventListener()
        {

        }

        /// <summary>
        /// 移除监听
        /// </summary>
        public virtual void RemoveEventListener()
        {

        }

        public void Dispose()
        {
            RemoveEventListener();
            CurrServerClient.Dispose();
        }
    }
}

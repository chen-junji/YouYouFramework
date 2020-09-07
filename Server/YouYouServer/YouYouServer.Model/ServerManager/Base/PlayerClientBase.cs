using System;
using System.Collections.Generic;
using System.Text;
using YouYouServer.Core;

namespace YouYouServer.Model
{
    public abstract class PlayerClientBase : IDisposable
    {
        /// <summary>
        /// 玩家账号
        /// </summary>
        public long AccountId;

        /// <summary>
        /// Socket事件监听派发器
        /// </summary>
        public EventDispatcher EventDispatcher
        {
            get;
            private set;
        }

        public PlayerClientBase()
        {
            EventDispatcher = new EventDispatcher();
        }

        public void Dispose()
        {

        }
    }
}

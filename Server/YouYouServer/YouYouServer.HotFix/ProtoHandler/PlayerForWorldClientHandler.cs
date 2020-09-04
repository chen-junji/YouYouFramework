using System;
using System.Collections.Generic;
using System.Reflection;
using YouYou.Proto;
using YouYouServer.Common;
using YouYouServer.Core.Common;
using YouYouServer.HotFix.Managers;
using YouYouServer.Model.IHandler;
using YouYouServer.Model.ServerManager;

namespace YouYouServer.HotFix.ProtoHandler
{
    [Handler(ConstDefine.PlayerForWorldClientHandler)]
    public class PlayerForWorldClientHandler : IPlayerForWorldClientHandler
    {
        /// <summary>
        /// 中心服务器上的玩家客户端
        /// </summary>
        private PlayerForWorldClient m_PlayerForWorldClient;

        private Dictionary<ushort, EventDispatcher.OnActionHandler> m_HandlerMessageDic;

        public void Init(PlayerForWorldClient playerForWorldClient)
        {
            m_PlayerForWorldClient = playerForWorldClient;
            AddEventListener();
        }

        #region AddEventListener 添加消息包监听
        /// <summary>
        /// 添加消息包监听
        /// </summary>
        public void AddEventListener()
        {
            m_HandlerMessageDic = new Dictionary<ushort, EventDispatcher.OnActionHandler>();

            //获取这个类上的所有方法
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            int num = 0;
            while (methods != null && num < methods.Length)
            {
                MethodInfo methodInfo = methods[num];
                string str = methodInfo.Name;
                object[] customAttributes = methodInfo.GetCustomAttributes(typeof(HandlerMessageAttribute), true);
                for (int i = 0; i < customAttributes.Length; i++)
                {
                    //找到带HandlerMessageAttribute属性标记的类 进行监听
                    HandlerMessageAttribute handlerMessage = customAttributes[i] as HandlerMessageAttribute;
                    if (null != handlerMessage)
                    {
                        EventDispatcher.OnActionHandler actionHandler = 
                            (EventDispatcher.OnActionHandler)Delegate.CreateDelegate(typeof(EventDispatcher.OnActionHandler), this, methodInfo);
                        m_HandlerMessageDic[handlerMessage.ProtoId] = actionHandler;
                        m_PlayerForWorldClient.EventDispatcher.AddEventListener(handlerMessage.ProtoId, actionHandler);
                    }
                }

                num++;
            }
        }
        #endregion

        #region RemoveEventListener 移除消息包监听
        /// <summary>
        /// 移除消息包监听
        /// </summary>
        public void RemoveEventListener()
        {
            var enumerator = m_HandlerMessageDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                m_PlayerForWorldClient.EventDispatcher.RemoveEventListener(enumerator.Current.Key, enumerator.Current.Value);
            }
            m_HandlerMessageDic.Clear();
            m_HandlerMessageDic = null;
        }
        #endregion

        [HandlerMessage(ProtoIdDefine.Proto_C2WS_CreateRole)]
        /// <summary>
        /// 客户端发送创建角色消息
        /// </summary>
        /// <param name="buffer"></param>
        private async void OnCreateRoleAsync(byte[] buffer)
        {
            C2WS_CreateRole createRoleProto = (C2WS_CreateRole)C2WS_CreateRole.Descriptor.Parser.ParseFrom(buffer);
            m_PlayerForWorldClient.CurrRole = await RoleManager.CreateRoleAsync(m_PlayerForWorldClient.AccountId, (byte)createRoleProto.JobId, (byte)createRoleProto.Sex, createRoleProto.NickName);

            WS2C_ReturnCreateRole retProto = new WS2C_ReturnCreateRole();
            if (m_PlayerForWorldClient.CurrRole == null)
            {
                //创建角色失败
                retProto.Result = false;
                retProto.RoleId = -100;
            }
            else
            {
                //创建角色成功
                retProto.Result = true;
                retProto.RoleId = m_PlayerForWorldClient.CurrRole.YFId;
            }

            m_PlayerForWorldClient.SendCarryToClient(retProto);
        }

        public void Dispose()
        {
            RemoveEventListener();
        }
    }
}

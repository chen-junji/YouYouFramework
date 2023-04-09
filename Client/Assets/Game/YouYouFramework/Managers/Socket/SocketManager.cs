using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// Socket管理器
    /// </summary>
    public class SocketManager : IDisposable
    {
        /// <summary>
        /// SocketTcp访问器链表
        /// </summary>
        private LinkedList<SocketTcpRoutine> m_SocketTcpRoutineList;

        [Header("每帧最大发送数量")]
        public int MaxSendCount = 5;

        [Header("每次发包最大字节数量")]
        public int MaxSendByteCount = 1024;

        [Header("每帧最大接收数量")]
        public int MaxReceiveCount = 5;

        [Header("心跳间隔 秒")]
        public int HeartbeatInterval = 10;

        /// <summary>
        /// 上次心跳时间
        /// </summary>
        private float m_PrevHeartbeatInterval = 0;

        /// <summary>
        /// PING值(毫秒)
        /// </summary>
        [HideInInspector]
        public int PingValue;

        /// <summary>
        /// 游戏服务器的时间
        /// </summary>
        [HideInInspector]
        public long GameServerTime;

        /// <summary>
        /// 和服务器对表的时刻
        /// </summary>
        [HideInInspector]
        public float CheckServerTime;

        /// <summary>
        /// 获取当前的Socket服务器时间
        /// </summary>
        /// <returns></returns>
        public long GetCurrServerTime()
        {
            return (int)((Time.realtimeSinceStartup - CheckServerTime) * 1000) + GameServerTime;
        }

        /// <summary>
        /// 是否已连接到了服务器
        /// </summary>
        private bool m_IsConnectToMainSocket = false;


        /// <summary>
        /// 发送数据的MemoryStream
        /// </summary>
        public MMO_MemoryStream SocketSendMS { get; private set; }
        /// <summary>
        /// 接收数据的MemoryStream
        /// </summary>
        public MMO_MemoryStream SocketReceiveMS { get; private set; }

        internal SocketManager()
        {
            m_SocketTcpRoutineList = new LinkedList<SocketTcpRoutine>();
            SocketSendMS = new MMO_MemoryStream();
            SocketReceiveMS = new MMO_MemoryStream();
        }
        internal void Init()
        {
            m_MainSocket = CreateSocketTcpRoutine();
            m_MainSocket.OnConnectOK = () =>
            {
                //已经建立了连接
                m_IsConnectToMainSocket = true;
                GameEntry.Event.Common.Dispatch(CommonEventId.OnConnectOKToMainSocket);
            };

            SocketProtoListener.AddProtoListener();
        }

        /// <summary>
        /// 创建SocketTcp访问器
        /// </summary>
        /// <returns></returns>
        public SocketTcpRoutine CreateSocketTcpRoutine()
        {
            //从对象池中取出访问器
            return GameEntry.Pool.DequeueClassObject<SocketTcpRoutine>();
        }

        /// <summary>
        /// 注册SocketTcp访问器
        /// </summary>
        /// <param name="routine"></param>
        internal void RegisterSocketTcpRoutine(SocketTcpRoutine routine)
        {
            m_SocketTcpRoutineList.AddFirst(routine);
        }

        /// <summary>
        /// 移除SocketTcp访问器
        /// </summary>
        /// <param name="routine"></param>
        internal void RemoveSocketTcpRoutine(SocketTcpRoutine routine)
        {
            m_SocketTcpRoutineList.Remove(routine);
        }

        internal void OnUpdate()
        {
            for (LinkedListNode<SocketTcpRoutine> curr = m_SocketTcpRoutineList.First; curr != null; curr = curr.Next)
            {
                curr.Value.OnUpdate();
            }

            if (m_IsConnectToMainSocket)
            {
                if (Time.realtimeSinceStartup > m_PrevHeartbeatInterval + HeartbeatInterval)
                {
                    //循环定时
                    m_PrevHeartbeatInterval = Time.realtimeSinceStartup;

                    //发送心跳
                    //System_HeartbeatProto proto = new System_HeartbeatProto();
                    //proto.LocalTime = Time.realtimeSinceStartup * 1000;
                    //CheckServerTime = Time.realtimeSinceStartup;
                    //SendMainMsg(proto.ToArray());
                }
            }
        }

        public void Dispose()
        {
            m_SocketTcpRoutineList.Clear();

            m_IsConnectToMainSocket = false;

            m_MainSocket.DisConnect();
            GameEntry.Pool.EnqueueClassObject(m_MainSocket);
            SocketProtoListener.RemoveProtoListener();

            SocketSendMS.Dispose();
            SocketReceiveMS.Dispose();

            SocketSendMS.Close();
            SocketReceiveMS.Close();
        }

        //=====================================
        /// <summary>
        /// 主Socket
        /// </summary>
        private SocketTcpRoutine m_MainSocket;

        /// <summary>
        /// 主Socket连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void ConnectToMainSocket(string ip, int port)
        {
            m_MainSocket.Connect(ip, port);
        }
        /// <summary>
        /// 主Socket发送消息
        /// </summary>
        /// <param name="buffer"></param>
        public void SendMainMsg(IProto proto)
        {
            GameEntry.Log(LogCategory.NetWork, "发送消息=={0}{1}", proto.ProtoEnName, proto.ToJson());
            m_MainSocket.SendMsg(proto);
        }
        /// <summary>
        /// Lua中发送消息
        /// </summary>
        /// <param name="protoId">消息编号</param>
        /// <param name="category">分类</param>
        /// <param name="buffer">消息体</param>
        public void SendMainMsgForLua(ushort protoId, byte category, byte[] buffer)
        {
            m_MainSocket.SendMsg(protoId, category, buffer);
        }
    }
}
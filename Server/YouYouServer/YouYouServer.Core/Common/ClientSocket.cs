using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YouYou;
using YouYouServer.Core.Common;
using YouYouServer.Core.Logger;
using YouYouServer.Core.Utils;

namespace YouYouServer.Core.Common
{
    public class ClientSocket
    {
        //事件派发器
        private EventDispatcher m_EventDispatcher;

        //Socket
        private Socket m_Socket;

        //作为服务器端监听客户端时候接收数据的线程
        private Thread m_ReceiveThread;


        #region 接收消息所需变量
        //接收数据包的字节数组缓冲区
        private byte[] m_ReceiveBuffer = new byte[2048];

        //接收数据包的缓冲数据流
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();

        /// <summary>
        /// 发送用的MS
        /// </summary>
        private MMO_MemoryStream m_SocketSendMS = new MMO_MemoryStream();

        /// <summary>
        /// 接收用的MS
        /// </summary>
        private MMO_MemoryStream m_SocketReceiveMS = new MMO_MemoryStream();

        #endregion

        #region 发送消息所需变量
        //发送消息队列
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();

        //压缩数组的长度界限
        private const int m_CompressLen = 200;
        #endregion

        /// <summary>
        /// 这个构造函数 是当做客户端使用时候使用
        /// </summary>
        /// <param name="eventDispatcher"></param>
        public ClientSocket(EventDispatcher eventDispatcher)
        {
            m_EventDispatcher = eventDispatcher;
        }


        /// <summary>
        /// 这个构造函数 是当做服务器端时候使用
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="role"></param>
        public ClientSocket(Socket socket, EventDispatcher eventDispatcher)
        {
            m_Socket = socket;
            m_EventDispatcher = eventDispatcher;

            //启动线程 进行接收数据
            m_ReceiveThread = new Thread(ReceiveMsg);
            m_ReceiveThread.Start();
        }

        #region ReceiveMsg 接收数据
        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMsg()
        {
            //异步接收数据
            m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Socket);
        }
        #endregion

        #region IsSocketConnected 判断socket是否连接
        /// <summary>
        /// 判断socket是否连接
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }
        #endregion

        #region ReceiveCallBack 接收数据回调
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if (IsSocketConnected(m_Socket))
                {
                    int len = m_Socket.EndReceive(ar);

                    if (len > 0)
                    {
                        //已经接收到数据

                        //把接收到数据 写入缓冲数据流的尾部
                        m_ReceiveMS.Position = m_ReceiveMS.Length;

                        //把指定长度的字节 写入数据流
                        m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);

                        //如果缓存数据流的长度>2 说明至少有个不完整的包过来了
                        //为什么这里是2 因为我们客户端封装数据包 用的ushort 长度就是2
                        if (m_ReceiveMS.Length > 2)
                        {
                            //进行循环 拆分数据包
                            while (true)
                            {
                                //把数据流指针位置放在0处
                                m_ReceiveMS.Position = 0;

                                //currMsgLen = 包体的长度
                                int currMsgLen = m_ReceiveMS.ReadUShort();

                                //currFullMsgLen 总包的长度=包头长度+包体长度
                                int currFullMsgLen = 2 + currMsgLen;

                                //如果数据流的长度>=整包的长度 说明至少收到了一个完整包
                                if (m_ReceiveMS.Length >= currFullMsgLen)
                                {
                                    //至少收到一个完整包

                                    //定义包体的byte[]数组
                                    byte[] buffer = new byte[currMsgLen];

                                    //把数据流指针放到2的位置 也就是包体的位置
                                    m_ReceiveMS.Position = 2;

                                    //把包体读到byte[]数组
                                    m_ReceiveMS.Read(buffer, 0, currMsgLen);

                                    //===================================================
                                    byte[] bufferNew = new byte[buffer.Length - 1];
                                    bool isCompress = false;

                                    MMO_MemoryStream ms1 = this.m_ReceiveMS;
                                    ms1.SetLength(0);
                                    ms1.Write(buffer, 0, buffer.Length);
                                    ms1.Position = 0;

                                    isCompress = ms1.ReadBool();
                                    ms1.Read(bufferNew, 0, bufferNew.Length);

                                    if (isCompress)
                                    {
                                        bufferNew = ZlibHelper.DeCompressBytes(bufferNew);
                                    }

                                    //协议编号
                                    ushort protoId = 0;
                                    //协议分类
                                    ProtoCategory protoCategory;
                                    byte[] protoContent = new byte[bufferNew.Length - 3];

                                    MMO_MemoryStream ms2 = this.m_ReceiveMS;
                                    ms2.SetLength(0);
                                    ms2.Write(bufferNew, 0, bufferNew.Length);
                                    ms2.Position = 0;

                                    protoId = ms2.ReadUShort();
                                    protoCategory = (ProtoCategory)ms2.ReadByte();
                                    ms2.Read(protoContent, 0, protoContent.Length);

                                    //异或 得到原始数据
                                    protoContent = SecurityUtil.Xor(protoContent);

                                    //这里判断 如果是玩家发给游戏服或者中心服务器的消息 进行二次封装
                                    if (protoCategory == ProtoCategory.Client2GameServer ||
                                        protoCategory == ProtoCategory.Client2WorldServer ||
                                        protoCategory == ProtoCategory.CarryProto
                                        )
                                    {
                                        //网关服务处理中转协议
                                        OnCarryProto?.Invoke(protoId, protoCategory, protoContent);
                                    }
                                    else
                                    {
                                        m_EventDispatcher.Dispatch(protoId, protoContent);
                                    }
                                }

                                //==============处理剩余字节数组===================

                                //剩余字节长度
                                int remainLen = (int)m_ReceiveMS.Length - currFullMsgLen;
                                if (remainLen > 0)
                                {
                                    //把指针放在第一个包的尾部
                                    m_ReceiveMS.Position = currFullMsgLen;

                                    //定义剩余字节数组
                                    byte[] remainBuffer = new byte[remainLen];

                                    //把数据流读到剩余字节数组
                                    m_ReceiveMS.Read(remainBuffer, 0, remainLen);

                                    //清空数据流
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);

                                    //把剩余字节数组重新写入数据流
                                    m_ReceiveMS.Write(remainBuffer, 0, remainBuffer.Length);

                                    remainBuffer = null;
                                }
                                else
                                {
                                    //没有剩余字节

                                    //清空数据流
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);

                                    break;
                                }
                            }
                        }

                        //进行下一次接收数据包
                        ReceiveMsg();
                    }
                    else
                    {
                        //客户端断开连接
                        OnDisConnect?.Invoke();
                        LoggerMgr.Log(Core.LoggerLevel.LogError, 0, "客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());
                    }
                }
                else
                {
                    //客户端断开连接
                    OnDisConnect?.Invoke();
                    LoggerMgr.Log(Core.LoggerLevel.LogError, 0, "客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());
                }

            }
            catch (Exception ex)
            {
                //客户端断开连接
                OnDisConnect?.Invoke();
                LoggerMgr.Log(Core.LoggerLevel.LogError, 0, "客户端{0}断开连接 原因{1}", m_Socket.RemoteEndPoint.ToString(), ex.Message);
            }
        }
        #endregion


        //========================================================


        #region OnCheckSendQueueCallBack 检查队列的委托回调
        /// <summary>
        /// 检查队列的委托回调
        /// </summary>
        private void OnCheckSendQueueCallBack()
        {
            lock (m_SendQueue)
            {
                //如果队列中有数据包 则发送数据包
                if (m_SendQueue.Count > 0)
                {
                    //发送数据包
                    Send(m_SendQueue.Dequeue());
                }
            }
        }
        #endregion

        #region MakeData 封装数据包
        /// <summary>
        /// 封装数据包
        /// </summary>
        /// <param name="proto"></param>
        /// <returns></returns>
        private byte[] MakeData(IProto proto)
        {
            byte[] retBuffer = null;

            byte[] data = proto.ToByteArray();
            //1.如果数据包的长度 大于了m_CompressLen 则进行压缩
            bool isCompress = data.Length > m_CompressLen ? true : false;
            if (isCompress)
            {
                data = ZlibHelper.CompressBytes(data);
            }

            //2.异或
            data = SecurityUtil.Xor(data);

            MMO_MemoryStream ms = this.m_SocketSendMS;
            ms.SetLength(0);

            ms.WriteUShort((ushort)(data.Length + 4)); //4=isCompress 1 + ProtoId 2 + Category 1

            ms.WriteBool(isCompress);

            ms.WriteUShort(proto.ProtoId);
            ms.WriteByte((byte)proto.Category);

            ms.Write(data, 0, data.Length);

            retBuffer = ms.ToArray();
            return retBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="carryProto"></param>
        /// <returns></returns>
        private byte[] MakeData(CarryProto carryProto)
        {
            byte[] retBuffer = null;

            byte[] data = carryProto.ToArray();
            //1.如果数据包的长度 大于了m_CompressLen 则进行压缩
            bool isCompress = data.Length > m_CompressLen ? true : false;
            if (isCompress)
            {
                data = ZlibHelper.CompressBytes(data);
            }

            //2.异或
            data = SecurityUtil.Xor(data);

            MMO_MemoryStream ms = m_SocketSendMS;
            ms.SetLength(0);

            ms.WriteUShort((ushort)(data.Length + 4)); //4=isCompress 1 + ProtoId 2 + Category 1

            ms.WriteBool(isCompress);

            ms.WriteUShort(0);
            ms.WriteByte((byte)carryProto.Category);

            ms.Write(data, 0, data.Length);

            retBuffer = ms.ToArray();
            return retBuffer;
        }

        /// <summary>
        /// 封装数据包
        /// </summary>
        /// <param name="protoId"></param>
        /// <param name="category"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private byte[] MakeData(ushort protoId, byte category, byte[] buffer)
        {
            byte[] retBuffer = null;

            byte[] data = buffer;
            //1.如果数据包的长度 大于了m_CompressLen 则进行压缩
            bool isCompress = data.Length > m_CompressLen ? true : false;
            if (isCompress)
            {
                data = ZlibHelper.CompressBytes(data);
            }

            //2.异或
            data = SecurityUtil.Xor(data);

            MMO_MemoryStream ms = m_SocketSendMS;
            ms.SetLength(0);

            ms.WriteUShort((ushort)(data.Length + 4)); //4=isCompress 1 + ProtoId 2 + Category 1

            ms.WriteBool(isCompress);

            ms.WriteUShort(protoId);
            ms.WriteByte(category);

            ms.Write(data, 0, data.Length);

            retBuffer = ms.ToArray();
            return retBuffer;
        }
        #endregion

        #region SendMsg 发送消息 把消息加入队列
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="proto"></param>
        public void SendMsg(IProto proto)
        {
            //得到封装后的数据包
            byte[] sendBuffer = MakeData(proto);

            lock (m_SendQueue)
            {
                //把数据包加入队列
                m_SendQueue.Enqueue(sendBuffer);

                //检查队列
                OnCheckSendQueueCallBack();
            }
        }

        public void SendMsg(CarryProto carryProto)
        {
            //得到封装后的数据包
            byte[] sendBuffer = MakeData(carryProto);

            lock (m_SendQueue)
            {
                //把数据包加入队列
                m_SendQueue.Enqueue(sendBuffer);

                //检查队列
                OnCheckSendQueueCallBack();
            }
        }

        public void SendMsg(ushort protoId, byte category, byte[] buffer)
        {
            //得到封装后的数据包
            byte[] sendBuffer = MakeData(protoId, category, buffer);

            lock (m_SendQueue)
            {
                //把数据包加入队列
                m_SendQueue.Enqueue(sendBuffer);

                //检查队列
                OnCheckSendQueueCallBack();
            }
        }
        #endregion

        #region Send 真正发送数据包到服务器
        /// <summary>
        /// 真正发送数据包到服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void Send(byte[] buffer)
        {
            m_Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_Socket);
        }
        #endregion

        #region SendCallBack 发送数据包的回调
        /// <summary>
        /// 发送数据包的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            m_Socket.EndSend(ar);

            //继续检查队列
            OnCheckSendQueueCallBack();
        }
        #endregion

        //===================================================

        /// <summary>
        /// 连接成功
        /// </summary>
        public Action OnConnectSuccess;

        /// <summary>
        /// 连接失败
        /// </summary>
        public Action OnConnectFail;

        /// <summary>
        /// 断开连接
        /// </summary>
        public Action OnDisConnect;

        /// <summary>
        /// 处理中转协议
        /// </summary>
        public BaseAction<ushort, ProtoCategory, byte[]> OnCarryProto;

        #region Connect 连接到socket服务器
        /// <summary>
        /// 连接到socket服务器
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">端口号</param>
        public void Connect(string ip, int port)
        {
            //如果socket已经存在 并且处于连接中状态 则直接返回
            if (m_Socket != null && m_Socket.Connected) return;
            AddressFamily addressFamily = AddressFamily.InterNetwork;
            m_Socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_Socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallBack, m_Socket);

            }
            catch (Exception ex)
            {
                LoggerMgr.Log(Core.LoggerLevel.LogError, 0, "连接失败={0}", ex.Message);
            }
        }

        /// <summary>
        /// 连接完毕
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallBack(IAsyncResult ar)
        {
            if (m_Socket.Connected)
            {
                OnConnectSuccess?.Invoke();
                LoggerMgr.Log(Core.LoggerLevel.Log, 0, "socket连接成功");
                ReceiveMsg();
            }
            else
            {
                OnConnectFail?.Invoke();
                LoggerMgr.Log(Core.LoggerLevel.LogError, 0, "socket连接失败");
            }
            m_Socket.EndConnect(ar);
        }
        #endregion

        #region DisConnect 断开连接
        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            if (m_Socket != null && m_Socket.Connected)
            {
                m_Socket.Shutdown(SocketShutdown.Both);
                m_Socket.Close();
            }
        }
        #endregion
    }
}

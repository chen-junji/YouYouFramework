using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// SocketTcp访问器
	/// </summary>
	public class SocketTcpRoutine
	{
		#region 发送消息所需变量
		//发送消息队列
		private Queue<byte[]> m_SendQueue = new Queue<byte[]>();

		//压缩数组的长度界限
		private const int m_CompressLen = 200;

		//是否连接成功
		private bool m_IsConnectedOk;

		/// <summary>
		/// 当前帧发送数量
		/// </summary>
		private int m_SendCount = 0;

		/// <summary>
		/// 是否有未处理的字节
		/// </summary>
		private bool m_IsHasUnDealBytes = false;
		/// <summary>
		/// 未处理的字节
		/// </summary>
		private byte[] m_UnDealBytes = null;
		#endregion

		#region 接收消息所需变量
		//接收数据包的缓冲数据流
		private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
		//接收数据包的字节数组缓冲区
		private byte[] m_ReceiveBuffer = new byte[1024];

		//接收消息的队列
		private Queue<byte[]> m_ReceiveQueue = new Queue<byte[]>();

		/// <summary>
		/// 当前帧接收数量
		/// </summary>
		private int m_ReceiveCount = 0;
		#endregion

		/// <summary>
		/// 发送数据的MemoryStream
		/// </summary>
		private MMO_MemoryStream m_SocketSendMS = new MMO_MemoryStream();
		/// <summary>
		/// 接收数据的MemoryStream
		/// </summary>
		private MMO_MemoryStream m_SocketReceiveMS = new MMO_MemoryStream();

		/// <summary>
		/// 客户端socket
		/// </summary>
		private Socket m_Client;

		/// <summary>
		/// 连接成功时调用一次
		/// </summary>
		public Action OnConnectOK;

		internal void OnUpdate()
		{
			if (m_IsConnectedOk)
			{
				m_IsConnectedOk = false;
				if (OnConnectOK != null)
				{
					OnConnectOK();
				}
			}

			#region 从队列中获取数据
			while (true)
			{
				if (m_ReceiveCount <= GameEntry.Socket.MaxReceiveCount)
				{
					m_ReceiveCount++;
					lock (m_ReceiveQueue)
					{
						if (m_ReceiveQueue.Count > 0)
						{
							//得到队列中的数据包
							byte[] buffer = m_ReceiveQueue.Dequeue();

							//异或之后的数组
							byte[] bufferNew = new byte[buffer.Length - 3];

							bool isCompress = false;
							ushort crc = 0;

							//1.解析队列中的数据包
							MMO_MemoryStream ms1 = m_SocketReceiveMS;
							ms1.SetLength(0);
							ms1.Write(buffer, 0, buffer.Length);
							ms1.Position = 0;

							isCompress = ms1.ReadBool();
							crc = ms1.ReadUShort();
							ms1.Read(bufferNew, 0, bufferNew.Length);

							//2.crc校验
							int newCrc = Crc16.CalculateCrc16(bufferNew);

							if (newCrc == crc)
							{
								//异或 得到原始数据
								bufferNew = SecurityUtil.Xor(bufferNew);

								if (isCompress)
								{
									bufferNew = ZlibHelper.DeCompressBytes(bufferNew);
								}

								ushort protoCode = 0;
								ProtoCategory protoCategory;
								byte[] protoContent = new byte[bufferNew.Length - 3];//这里-3 是减去 protoCode长度+protoCategory长度

								MMO_MemoryStream ms2 = m_SocketReceiveMS;
								ms2.SetLength(0);
								ms2.Write(bufferNew, 0, bufferNew.Length);
								ms2.Position = 0;

								//协议编号
								protoCode = ms2.ReadUShort();
								protoCategory = (ProtoCategory)ms2.ReadByte();
								ms2.Read(protoContent, 0, protoContent.Length);

								GameEntry.Event.SocketEvent.Dispatch(protoCode, protoContent);
							}
							else
							{
								Debug.Log("crc校验错误");
								break;
							}
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					m_ReceiveCount = 0;
					break;
				}
			}
			#endregion

			CheckSendQueue();
		}


		#region Connect 连接到socket服务器
		/// <summary>
		/// 连接到socket服务器
		/// </summary>
		/// <param name="ip">ip</param>
		/// <param name="port">端口号</param>
		public void Connect(string ip, int port)
		{
			//如果socket已经存在 并且处于连接中状态 则直接返回
			if (m_Client != null && m_Client.Connected) return;

			string newServerIp = ip;
			AddressFamily addressFamily = AddressFamily.InterNetwork;

#if UNITY_IPHONE && !UNITY_EDITOR && SDKCHANNEL_APPLE_STORE
        AppleStoreInterface.GetIPv6Type(ip, port.ToString(), out newServerIp, out addressFamily);
#endif

			m_Client = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				m_Client.BeginConnect(new IPEndPoint(IPAddress.Parse(newServerIp), port), ConnectCallBack, m_Client);

			}
			catch (Exception ex)
			{
				Debug.Log("连接失败=" + ex.Message);
			}
		}
		/// <summary>
		/// 连接请求回调
		/// </summary>
		/// <param name="ar"></param>
		private void ConnectCallBack(IAsyncResult ar)
		{
			if (m_Client.Connected)
			{
				GameEntry.Socket.RegisterSocketTcpRoutine(this);

				ReceiveMsg();
				m_IsConnectedOk = true;
			}
			else
			{
				Debug.Log("socket连接失败");
			}
			m_Client.EndConnect(ar);
		}
		#endregion

		#region DisConnect 从当前连接的Socket服务器断开
		/// <summary>
		/// 从当前连接的Socket服务器断开
		/// </summary>
		public void DisConnect()
		{
			if (m_Client != null && m_Client.Connected)
			{
				m_Client.Shutdown(SocketShutdown.Both);
				m_Client.Close();
				GameEntry.Socket.RemoveSocketTcpRoutine(this);
				Debug.Log("从当前连接的Socket服务器断开");
			}
		}
		#endregion

		#region CheckSendQueue 检查发送队列
		/// <summary>
		/// 检查发送队列
		/// </summary>
		private void CheckSendQueue()
		{
			if (m_SendCount >= GameEntry.Socket.MaxSendCount)
			{
				//等待下一帧发送
				m_SendCount = 0;
				return;
			}

			lock (m_SendQueue)
			{
				if (m_SendQueue.Count > 0 || m_IsHasUnDealBytes)
				{
					MMO_MemoryStream ms = m_SocketSendMS;
					ms.SetLength(0);

					if (m_IsHasUnDealBytes)
					{
						m_IsHasUnDealBytes = false;
						ms.Write(m_UnDealBytes, 0, m_UnDealBytes.Length);
					}

					while (true)
					{
						if (m_SendQueue.Count == 0) break;

						//取出一个字节数组
						byte[] buffer = m_SendQueue.Dequeue();
						if (buffer.Length + ms.Length <= GameEntry.Socket.MaxSendByteCount)
						{
							ms.Write(buffer, 0, buffer.Length);
						}
						else
						{
							//已经取出来一个要发送的字节数组
							m_UnDealBytes = buffer;
							m_IsHasUnDealBytes = true;
							break;
						}
					}

					m_SendCount++;
					Send(ms.ToArray());
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

			MMO_MemoryStream ms = m_SocketSendMS;
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
		/// <param name="buffer"></param>
		public void SendMsg(IProto proto)
		{
			//得到封装后的数据包
			byte[] sendBuffer = MakeData(proto);

			lock (m_SendQueue)
			{
				//把数据包加入队列
				m_SendQueue.Enqueue(sendBuffer);
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
			m_Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_Client);
		}
		#endregion

		#region SendCallBack 发送数据包的回调
		/// <summary>
		/// 发送数据包的回调
		/// </summary>
		/// <param name="ar"></param>
		private void SendCallBack(IAsyncResult ar)
		{
			m_Client.EndSend(ar);
		}
		#endregion

		//====================================================

		#region ReceiveMsg 接收数据
		/// <summary>
		/// 接收数据
		/// </summary>
		private void ReceiveMsg()
		{
			//异步接收数据
			m_Client.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Client);
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
				int len = m_Client.EndReceive(ar);

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

								lock (m_ReceiveQueue)
								{
									m_ReceiveQueue.Enqueue(buffer);
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
							else
							{
								//还没有收到完整包
								break;
							}
						}
					}

					//进行下一次接收数据包
					ReceiveMsg();
				}
				else
				{
					//服务器断开连接
					Debug.Log(string.Format("服务器{0}断开连接", m_Client.RemoteEndPoint.ToString()));
				}
			}
			catch
			{
				//服务器断开连接
				Debug.Log(string.Format("服务器{0}断开连接", m_Client.RemoteEndPoint.ToString()));
			}
		}
		#endregion
	}
}
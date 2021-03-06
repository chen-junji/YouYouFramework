using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YouYou
{
	public class WebSocketManager : ManagerBase, IDisposable
	{
		private ClientWebSocket m_WebSocket;
		private CancellationToken m_Cancellation;


		internal WebSocketManager()
		{
			
		}
		internal override void Init()
		{
			m_WebSocket = new ClientWebSocket();
			m_Cancellation = new CancellationToken();
		}
		internal void OnUpdate()
		{

		}

		internal async void ConnectToWebSocket(string url, Action onComplete = null)
		{
			if (m_WebSocket.State == WebSocketState.Open) return;
			m_WebSocket = new ClientWebSocket();
			m_Cancellation = new CancellationToken();

			await m_WebSocket.ConnectAsync(new Uri(url), m_Cancellation);
			if (m_WebSocket.State == WebSocketState.Open)
			{
				onComplete?.Invoke();
			}

			while (true)
			{
				var result = new byte[10240];
				ArraySegment<byte> arraySegment = new ArraySegment<byte>(result);
				Task<WebSocketReceiveResult> taskResult = m_WebSocket.ReceiveAsync(arraySegment, new CancellationToken());//接受数据
				await taskResult;
				string json = string.Empty;
				if (taskResult.IsCompleted)
				{
					WebSocketReceiveResult tempResult = taskResult.Result;
					byte[] temp = new byte[tempResult.Count];
					Array.Copy(arraySegment.Array, temp, tempResult.Count);
					json = Encoding.UTF8.GetString(temp, 0, temp.Length);
				}
				result = null;

				string method = json.JsonCutApart("Method");
#if DEBUG_LOG_PROTO && DEBUG_MODEL
				GameEntry.Log(LogCategory.Proto, "WebSocket接收消息{0}==>>{1}", method, json);
#endif
				if (int.Parse(json.JsonCutApart("Status")) == 1)
				{
					string content = json.JsonCutApart("Content");
					if (!string.IsNullOrEmpty(content)) GameEntry.Event.WebSocketEvent.Dispatch(method, content);
				}
				else
				{
					Debug.Log(json.JsonCutApart("Msg"));
				}
			}
		}
		public void CloseWebSocket()
		{
			if (m_WebSocket.State != WebSocketState.None) m_WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", m_Cancellation);
		}

		public void Dispose()
		{
			CloseWebSocket();
		}
	}
}

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


		public WebSocketManager()
		{
			m_WebSocket = new ClientWebSocket();
			m_Cancellation = new CancellationToken();
		}
		public override void Init()
		{

		}
		internal void OnUpdate()
		{

		}

		public async void ConnectToWebSocket(string url, Action onComplete = null)
		{
			if (m_WebSocket.State == WebSocketState.Open) return;
			await m_WebSocket.ConnectAsync(new Uri(url), m_Cancellation);
			if (m_WebSocket.State == WebSocketState.Open)
			{
				onComplete?.Invoke();
			}

			while (true)
			{
				var result = new byte[1024];
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


#if DEBUG_LOG_PROTO && DEBUG_MODEL
				GameEntry.Log(LogCategory.Proto, "WebSocket接收消息==>>" + json);
#endif
				if (int.Parse(json.JsonCutApart("Status")) == 1)
				{
					string content = json.JsonCutApart("Content");
					if (!string.IsNullOrEmpty(content)) GameEntry.Event.WebSocketEvent.Dispatch(json.JsonCutApart("Method"), content);
				}
				else
				{
					Debug.Log(json.JsonCutApart("Msg"));
				}
			}
		}

		public void Dispose()
		{
			if (m_WebSocket.State != WebSocketState.None) m_WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", m_Cancellation);
		}
	}
}

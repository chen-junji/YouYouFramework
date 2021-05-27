using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class HttpManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// 正式账号服务器Url
		/// </summary>
		private string m_WebAccountUrl;
		/// <summary>
		/// 测试账号服务器Url
		/// </summary>
		private string m_TestWebAccountUrl;
		/// <summary>
		/// 是否测试环境
		/// </summary>
		private bool m_IsTest;
		/// <summary>
		/// 真实账号服务器Url
		/// </summary>
		public string RealWebAccountUrl { get { return "http://" + RealIpAndPort + "/"; } }
		public string RealIpAndPort { get { return m_IsTest ? m_TestWebAccountUrl : m_WebAccountUrl; } }
		/// <summary>
		/// 连接失败后重试次数
		/// </summary>
		public int Retry { get; private set; }
		/// <summary>
		/// 连接失败后重试间隔（秒）
		/// </summary>
		public int RetryInterval { get; private set; }

		public void Dispose()
		{

		}
		internal override void Init()
		{
			m_WebAccountUrl = GameEntry.ParamsSettings.WebAccountUrl;
			m_TestWebAccountUrl = GameEntry.ParamsSettings.TestWebAccountUrl;
			m_IsTest = GameEntry.ParamsSettings.IsTest;

			Retry = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_Retry, GameEntry.CurrDeviceGrade);
			RetryInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_RetryInterval, GameEntry.CurrDeviceGrade);
		}

		#region Get
		public void Get(string url, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
		{
			HttpRoutine http = GameEntry.Pool.DequeueClassObject<HttpRoutine>();
			if (loadingCircle)
			{
				GameEntry.Task.Add((Action taskComplete) =>
				{
					http.Get(url, (HttpCallBackArgs ret) =>
					{
						taskComplete?.Invoke();
						callBack?.Invoke(ret);
					});
				});
			}
			else
			{
				http.Get(url, callBack);
			}
		}
		#endregion

		#region Post
		public void PostArgs(string url, string json = null, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
		{
			HttpRoutine http = GameEntry.Pool.DequeueClassObject<HttpRoutine>();
			if (loadingCircle)
			{
				GameEntry.Task.Add((Action taskComplete) =>
				{
					http.Post(url, json, (HttpCallBackArgs ret) =>
					{
						taskComplete?.Invoke();
						callBack?.Invoke(ret);
					});
				});
			}
			else
			{
				http.Post(url, json, callBack);
			}
		}
		public void Post(string url, string json = null, bool loadingCircle = false, Action<string> callBack = null)
		{
			PostArgs(url, json, loadingCircle, (args) =>
			{
				if (!args.HasError && args.Value.JsonCutApart("Status").ToInt() == 1)
				{
					callBack?.Invoke(args.Value.JsonCutApart("Content"));
				}
				else
				{
					Debug.LogError(args.Value.JsonCutApart("Msg"));
				}
			});
		}
		#endregion
	}
}
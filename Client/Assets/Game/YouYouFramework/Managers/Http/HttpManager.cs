using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class HttpManager 
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


        internal void Init()
        {
            m_WebAccountUrl = MainEntry.ParamsSettings.WebAccountUrl;
            m_TestWebAccountUrl = MainEntry.ParamsSettings.TestWebAccountUrl;
            m_IsTest = Main.MainEntry.ParamsSettings.IsTest;

        }

        #region Get
        public void GetArgs(string url, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
        {
            GameEntry.Task.AddTaskCommon((taskRoutine) =>
            {
                MainEntry.ClassObjectPool.Dequeue<HttpRoutine>().Get(url, (HttpCallBackArgs ret) =>
                {
                    taskRoutine.Leave();
                    if (ret.HasError)
                    {
                        Debug.LogError("UITipLogOut");
                        //GameEntry.Instance.UITipLogOut.SetValue(() => GetArgs(url, loadingCircle, callBack));
                    }
					else
                    {
                        callBack?.Invoke(ret);
                    }
                });
            }, loadingCircle);
        }
        public void Get(string url, bool loadingCircle = false, Action<string> callBack = null)
        {
            GetArgs(url, loadingCircle, (args) =>
            {
                if (args.Value.JsonCutApart("Status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("Content"));
            });
        }
        public async ETTask<HttpCallBackArgs> GetArgsAsync(string url, bool loadingCircle = false)
        {
            ETTask<HttpCallBackArgs> task = ETTask<HttpCallBackArgs>.Create();
            GetArgs(url, loadingCircle, task.SetResult);
            return await task;
        }
        public async ETTask<string> GetAsync(string url, bool loadingCircle = false)
        {
            ETTask<string> task = ETTask<string>.Create();
            Get(url, loadingCircle, task.SetResult);
            return await task;
        }
        #endregion

        #region Post
        public void PostArgs(string url, string json = null, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
        {
            GameEntry.Task.AddTaskCommon((taskRoutine) =>
            {
                MainEntry.ClassObjectPool.Dequeue<HttpRoutine>().Post(url, json, (HttpCallBackArgs ret) =>
                {
                    taskRoutine.Leave();
                    if (ret.HasError)
                    {
                        Debug.LogError("UITipLogOut");
                        //GameEntry.Instance.UITipLogOut.SetValue(() => PostArgs(url, json, loadingCircle, callBack));
                    }
					else
                    {
                        callBack?.Invoke(ret);
                    }
                });
            }, loadingCircle);
        }
        public void Post(string url, string json = null, bool loadingCircle = false, Action<string> callBack = null)
        {
            PostArgs(url, json, loadingCircle, (args) =>
            {
                if (args.Value.JsonCutApart("Status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("Content"));
            });
        }
        public async ETTask<HttpCallBackArgs> PostArgsAsync(string url, string json = null, bool loadingCircle = false)
        {
            ETTask<HttpCallBackArgs> task = ETTask<HttpCallBackArgs>.Create();
            PostArgs(url, json, loadingCircle, task.SetResult);
            return await task;
        }
        public async ETTask<string> PostAsync(string url, string json = null, bool loadingCircle = false)
        {
            ETTask<string> task = ETTask<string>.Create();
            Post(url, json, loadingCircle, task.SetResult);
            return await task;
        }
        #endregion
    }
}
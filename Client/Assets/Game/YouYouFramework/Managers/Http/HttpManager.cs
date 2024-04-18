using Cysharp.Threading.Tasks;
using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
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


        public HttpManager()
        {
            m_WebAccountUrl = MainEntry.ParamsSettings.WebAccountUrl;
            m_TestWebAccountUrl = MainEntry.ParamsSettings.TestWebAccountUrl;
            m_IsTest = MainEntry.ParamsSettings.IsTest;
        }

        #region Get
        public void GetArgs(string url, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
        {
            if (loadingCircle)
            {
                CircleCtrl.Instance.CircleOpen();
            }

            HttpRoutine.Create().Get(url, (HttpCallBackArgs ret) =>
            {
                if (loadingCircle)
                {
                    CircleCtrl.Instance.CircleClose();
                }

                if (ret.HasError)
                {
                    DialogForm.ShowForm(ret.Value, "网络请求错误");
                }
                else
                {
                    callBack?.Invoke(ret);
                }
            });
        }
        public void Get(string url, bool loadingCircle = false, Action<string> callBack = null)
        {
            GetArgs(url, loadingCircle, (args) =>
            {
                if (args.Value.JsonCutApart("Status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("Content"));
            });
        }
        public UniTask<HttpCallBackArgs> GetArgsAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<HttpCallBackArgs>();
            GetArgs(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        public UniTask<string> GetAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Get(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        #endregion

        #region Post
        public void PostArgs(string url, string json = null, bool loadingCircle = false, HttpSendDataCallBack callBack = null)
        {
            if (loadingCircle)
            {
                CircleCtrl.Instance.CircleOpen();
            }

            HttpRoutine.Create().Post(url, json, (HttpCallBackArgs ret) =>
            {
                if (loadingCircle)
                {
                    CircleCtrl.Instance.CircleClose();
                }

                if (ret.HasError)
                {
                    DialogForm.ShowForm(ret.Value, "网络请求错误");
                }
                else
                {
                    callBack?.Invoke(ret);
                }
            });
        }
        public void Post(string url, string json = null, bool loadingCircle = false, Action<string> callBack = null)
        {
            PostArgs(url, json, loadingCircle, (args) =>
            {
                if (args.Value.JsonCutApart("Status").ToInt() == 1) callBack?.Invoke(args.Value.JsonCutApart("Content"));
            });
        }
        public UniTask<HttpCallBackArgs> PostArgsAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<HttpCallBackArgs>();
            PostArgs(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        public UniTask<string> PostAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Post(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        #endregion
    }
}
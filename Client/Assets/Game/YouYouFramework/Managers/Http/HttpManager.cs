using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace YouYouFramework
{
    public class HttpManager
    {
        /// <summary>
        /// 正式账号服务器Url
        /// </summary>
        private string webAccountUrl;
        /// <summary>
        /// 测试账号服务器Url
        /// </summary>
        private string testWebAccountUrl;
        /// <summary>
        /// 是否测试环境
        /// </summary>
        private bool isTest;
        /// <summary>
        /// 真实账号服务器Url
        /// </summary>
        public string RealWebAccountUrl { get { return "http://" + RealIpAndPort + "/"; } }
        public string RealIpAndPort { get { return isTest ? testWebAccountUrl : webAccountUrl; } }


        public HttpManager()
        {
            webAccountUrl = GameEntry.ParamsSettings.WebAccountUrl;
            testWebAccountUrl = GameEntry.ParamsSettings.TestWebAccountUrl;
            isTest = GameEntry.ParamsSettings.IsTest;
        }

        public void GetArgs(string url, bool loadingCircle = false, Action<UnityWebRequest> callBack = null)
        {
            if (loadingCircle)
            {
                CircleCtrl.Instance.CircleOpen();
            }

            HttpRoutine.Create().Get(url, (ret) =>
            {
                if (loadingCircle)
                {
                    CircleCtrl.Instance.CircleClose();
                }

                if (ret.result != UnityWebRequest.Result.Success)
                {
                    //DialogForm.ShowForm(ret.Value, "网络请求错误");
                    if (GameEntry.DataTable.Sys_DialogDBModel.keyDic.TryGetValue("Error404", out var entity))
                    {
                        DialogForm.ShowFormByKey("Error404");
                    }
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
                callBack?.Invoke(args.downloadHandler.text);
            });
        }
        public UniTask<UnityWebRequest> GetArgsAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<UnityWebRequest>();
            GetArgs(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        public UniTask<string> GetAsync(string url, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Get(url, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }

        public void PostArgs(string url, string json = null, bool loadingCircle = false, Action<UnityWebRequest> callBack = null)
        {
            if (loadingCircle)
            {
                CircleCtrl.Instance.CircleOpen();
            }

            HttpRoutine.Create().Post(url, json, (ret) =>
            {
                if (loadingCircle)
                {
                    CircleCtrl.Instance.CircleClose();
                }

                if (ret.result != UnityWebRequest.Result.Success)
                {
                    //DialogForm.ShowForm(ret.Value, "网络请求错误");
                    if (GameEntry.DataTable.Sys_DialogDBModel.keyDic.TryGetValue("Error404", out var entity))
                    {
                        DialogForm.ShowFormByKey("Error404");
                    }
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
                callBack?.Invoke(args.downloadHandler.text);
            });
        }
        public UniTask<UnityWebRequest> PostArgsAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<UnityWebRequest>();
            PostArgs(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }
        public UniTask<string> PostAsync(string url, string json = null, bool loadingCircle = false)
        {
            var task = new UniTaskCompletionSource<string>();
            Post(url, json, loadingCircle, x => task.TrySetResult(x));
            return task.Task;
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace YouYouFramework
{
    /// <summary>
    /// Http访问器
    /// </summary>
    public class HttpRoutine
    {
        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// 当前重试次数
        /// </summary>
        private int currRetry = 0;

        /// <summary>
        /// 请求的url
        /// </summary>
        private string url;
        /// <summary>
        /// Post请求的json参数
        /// </summary>
        private string json;

        /// <summary>
        /// 请求回调
        /// </summary>
        private Action<UnityWebRequest> callBack;
        
        public static HttpRoutine Create()
        {
            return GameEntry.Pool.ClassObjectPool.Dequeue<HttpRoutine>();
        }

        /// <summary>
        /// 发送web数据
        /// </summary>
        public void Get(string url, Action<UnityWebRequest> callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;

            this.url = url;
            this.callBack = callBack;

            GetUrl(this.url);
        }

        public void Post(string url, string json = null, Action<UnityWebRequest> callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;

            this.url = url;
            this.callBack = callBack;
            this.json = json;

            PostUrl(this.url);
        }

        /// <summary>
        /// Get请求
        /// </summary>
        private void GetUrl(string url)
        {
            GameEntry.Log(LogCategory.NetWork, string.Format("Get请求:{0}, {1}次重试", this.url, currRetry));
            UnityWebRequest data = UnityWebRequest.Get(url);
            GameEntry.Instance.StartCoroutine(Request(data));
        }

        /// <summary>
        /// Post请求
        /// </summary>
        private void PostUrl(string url)
        {
            UnityWebRequest unityWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrWhiteSpace(json))
            {
                unityWeb.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

                if (!string.IsNullOrWhiteSpace(GameEntry.ParamsSettings.PostContentType))
                    unityWeb.SetRequestHeader("Content-Type", GameEntry.ParamsSettings.PostContentType);
            }

            GameEntry.Log(LogCategory.NetWork, $"Post请求:{this.url}, {currRetry}次重试==>>{json}");
            GameEntry.Instance.StartCoroutine(Request(unityWeb));
        }

        /// <summary>
        /// 请求服务器
        /// </summary>
        private IEnumerator Request(UnityWebRequest data)
        {
            data.timeout = 5;
            yield return data.SendWebRequest();
            if (data.result == UnityWebRequest.Result.Success)
            {
                IsBusy = false;
            }
            else
            {
                //报错了 进行重试
                if (currRetry > 0) yield return new WaitForSeconds(GameEntry.ParamsSettings.HttpRetryInterval);
                currRetry++;
                if (currRetry <= GameEntry.ParamsSettings.HttpRetry)
                {
                    switch (data.method)
                    {
                        case UnityWebRequest.kHttpVerbGET:
                            GetUrl(url);
                            break;
                        case UnityWebRequest.kHttpVerbPOST:
                            PostUrl(url);
                            break;
                    }
                    yield break;
                }

                IsBusy = false;
            }

            if (!string.IsNullOrWhiteSpace(data.downloadHandler.text))
            {
                GameEntry.Log(LogCategory.NetWork, string.Format("WebAPI回调:{0}, ==>>{1}", url, data.downloadHandler.text));
            }
            callBack?.Invoke(data);

            currRetry = 0;
            url = null;
            data.Dispose();

            //Debug.Log("把http访问器回池");
            GameEntry.Pool.ClassObjectPool.Enqueue(this);
        }

    }
}
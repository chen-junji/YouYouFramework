using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YouYouFramework;

namespace YouYouMain
{
    /// <summary>
    /// Http发送数据的回调委托
    /// </summary>
    /// <param name="args"></param>
    public delegate void HttpSendDataCallBack(HttpCallBackArgs args);

    /// <summary>
    /// Http访问器
    /// </summary>
    public class HttpRoutine
    {
        #region 属性

        /// <summary>
        /// Http请求回调
        /// </summary>
        private HttpSendDataCallBack m_CallBack;

        /// <summary>
        /// Http请求回调数据
        /// </summary>
        private HttpCallBackArgs m_CallBackArgs;

        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// 当前重试次数
        /// </summary>
        private int m_CurrRetry = 0;

        private string m_Url;
        private string m_Json;

        /// <summary>
        /// 发送的数据
        /// </summary>
        private Dictionary<string, object> m_Dic;
        #endregion

        public HttpRoutine()
        {
            m_CallBackArgs = new HttpCallBackArgs();
            m_Dic = new Dictionary<string, object>();
        }
        public static HttpRoutine Create()
        {
            return MainEntry.ClassObjectPool.Dequeue<HttpRoutine>();
        }

        #region SendData 发送web数据
        /// <summary>
        /// 发送web数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callBack"></param>
        /// <param name="isPost"></param>
        /// <param name="isGetData">是否获取字节数据</param>
        /// <param name="dic"></param>
        public void Get(string url, HttpSendDataCallBack callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;

            m_Url = url;
            m_CallBack = callBack;

            GetUrl(m_Url);
        }

        public void Post(string url, string json = null, HttpSendDataCallBack callBack = null)
        {
            if (IsBusy) return;
            IsBusy = true;

            m_Url = url;
            m_CallBack = callBack;
            m_Json = json;

            PostUrl(m_Url);
        }
        #endregion

        #region GetUrl Get请求
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        private void GetUrl(string url)
        {
            GameEntry.Log(LogCategory.NetWork, "Get请求:{0}, {1}次重试", m_Url, m_CurrRetry);
            UnityWebRequest data = UnityWebRequest.Get(url);
            GameEntry.Instance.StartCoroutine(Request(data));
        }
        #endregion

        #region PostUrl Post请求
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        private void PostUrl(string url)
        {
            UnityWebRequest unityWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrWhiteSpace(m_Json))
            {
                if (MainEntry.ParamsSettings.PostIsEncrypt && m_CurrRetry == 0)
                {
                    m_Dic["value"] = m_Json;
                    //web加密
                    m_Dic["deviceIdentifier"] = DeviceUtil.DeviceIdentifier;
                    m_Dic["deviceModel"] = DeviceUtil.DeviceModel;
                    long t = SystemModel.Instance.CurrServerTime;
                    m_Dic["sign"] = EncryptUtil.Md5(string.Format("{0}:{1}", t, DeviceUtil.DeviceIdentifier));
                    m_Dic["t"] = t;

                    m_Json = m_Dic.ToJson();
                }
                unityWeb.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(m_Json));

                if (!string.IsNullOrWhiteSpace(MainEntry.ParamsSettings.PostContentType))
                    unityWeb.SetRequestHeader("Content-Type", MainEntry.ParamsSettings.PostContentType);
            }

            GameEntry.Log(LogCategory.NetWork, "Post请求:{0}, {1}次重试==>>{2}", m_Url, m_CurrRetry, m_Json);
            GameEntry.Instance.StartCoroutine(Request(unityWeb));
        }
        #endregion

        #region Request 请求服务器
        /// <summary>
        /// 请求服务器
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private IEnumerator Request(UnityWebRequest data)
        {
            data.timeout = 5;
            yield return data.SendWebRequest();
            if (data.result == UnityWebRequest.Result.Success)
            {
                IsBusy = false;
                m_CallBackArgs.HasError = false;
                m_CallBackArgs.Value = data.downloadHandler.text;
                m_CallBackArgs.Data = data.downloadHandler.data;
            }
            else
            {
                //报错了 进行重试
                if (m_CurrRetry > 0) yield return new WaitForSeconds(MainEntry.ParamsSettings.HttpRetryInterval);
                m_CurrRetry++;
                if (m_CurrRetry <= MainEntry.ParamsSettings.HttpRetry)
                {
                    switch (data.method)
                    {
                        case UnityWebRequest.kHttpVerbGET:
                            GetUrl(m_Url);
                            break;
                        case UnityWebRequest.kHttpVerbPOST:
                            PostUrl(m_Url);
                            break;
                    }
                    yield break;
                }

                IsBusy = false;
                m_CallBackArgs.HasError = true;
                m_CallBackArgs.Value = data.error;
            }

            if (!string.IsNullOrWhiteSpace(m_CallBackArgs.Value))
            {
                GameEntry.Log(LogCategory.NetWork, "WebAPI回调:{0}, ==>>{1}", m_Url, m_CallBackArgs.ToJson());
            }
            m_CallBack?.Invoke(m_CallBackArgs);

            m_CurrRetry = 0;
            m_Url = null;
            if (m_Dic != null)
            {
                m_Dic.Clear();
                MainEntry.ClassObjectPool.Enqueue(m_Dic);
            }
            m_CallBackArgs.Data = null;
            data.Dispose();
            data = null;

            //Debug.Log("把http访问器回池");
            MainEntry.ClassObjectPool.Enqueue(this);
        }
        #endregion
    }
}
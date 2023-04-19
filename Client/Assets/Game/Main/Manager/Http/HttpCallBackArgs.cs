using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Http请求回调数据类 用于存储从Http站点下载的数据
/// </summary>
public class HttpCallBackArgs : EventArgs
{
    /// <summary>
    /// 是否有错
    /// </summary>
    public bool HasError;

    /// <summary>
    /// Json返回值
    /// </summary>
    public string Value;

    /// <summary>
    /// bytes返回值
    /// </summary>
    public byte[] Data;
}
//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2016-05-09 23:09:52
//备    注：
//===================================================
using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// 加密
/// </summary>
public sealed class EncryptUtil
{
    public static string Md5(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bytResult = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(value));
        string strResult = BitConverter.ToString(bytResult);
        strResult = strResult.Replace("-", "");
        return strResult;
    }

    /// <summary>
    /// 获取文件的MD5
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetFileMD5(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }
        try
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytResult = md5.ComputeHash(file);
            string strResult = BitConverter.ToString(bytResult);
            strResult = strResult.Replace("-", "");
            return strResult;
        }
        catch
        {
            return null;
        }
    }
}
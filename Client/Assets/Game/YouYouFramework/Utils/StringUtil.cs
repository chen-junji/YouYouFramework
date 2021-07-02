//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2015-11-16 22:26:09
//备    注：
//===================================================
using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// </summary>
public static class StringUtil
{
    #region IsNullOrEmpty 验证值是否为null

    /// <summary>
    /// 判断对象是否为Null、DBNull、Empty或空白字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(string value)
    {
        bool retVal = false;
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            retVal = true;
        }
        return retVal;
    }

    #endregion

    public static bool IsEmail(this string email)
    {
        Regex regex = new Regex("[a-zA-Z_0-9]+@[a-zA-Z_0-9]{2,6}(\\.[a-zA-Z_0-9]{2,3})+");
        return regex.IsMatch(email);
    }
    public static bool IsPhoneNumber(this string strInput)
    {
        Regex reg = new Regex(@"(^\d{11}$)");
        return reg.IsMatch(strInput);
    }
    public static string GetSplitRange(this string str)
    {
        string[] temps = str.Split('|');
        if (temps != null) return temps[Random.Range(0, temps.Length)];
        return str;
    }
    /// <summary>
    /// 检查后缀名
    /// </summary>
    public static bool IsSuffix(this string str, string suffix)
    {
        //总长度减去后缀的索引等于后缀的长度
        int indexOf = str.LastIndexOf(suffix);
        return indexOf != -1 && indexOf == str.Length - suffix.Length;
    }

    /// <summary>
    /// 把string类型转换成int
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(this string str)
    {
        int temp = 0;
        int.TryParse(str, out temp);
        return temp;
    }

    /// <summary>
    /// 把string类型转换成long
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static long ToLong(this string str)
    {
        long temp = 0;
        long.TryParse(str, out temp);
        return temp;
    }

    /// <summary>
    /// 把string类型转换成float
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float ToFloat(this string str)
    {
        float temp = 0;
        float.TryParse(str, out temp);
        return temp;
    }
}
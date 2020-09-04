//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2015-11-16 22:26:09
//备    注：
//===================================================
using UnityEngine;
using System.Collections;

/// <summary>
/// string拓展类
/// </summary>
public static class StringUtil 
{
    /// <summary>
    /// 扩展方法--将string转化为int
    /// <summary>
    public static int ToInt(this string str)
    {
        int temp = 0;
        int.TryParse(str, out temp);
        return temp;
    }

    /// <summary>
    /// 扩展方法--将string转化为long
    /// <summary>
    public static long ToLong(this string str)
    {
        long temp = 0;
        long.TryParse(str, out temp);
        return temp;
    }

    /// <summary>
    /// 扩展方法--将string转化为float
    /// <summary>
    public static float ToFloat(this string str)
    {
        float temp = 0;
        float.TryParse(str, out temp);
        return temp;
    }
}
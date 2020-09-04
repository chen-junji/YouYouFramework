//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2016-03-03 22:00:24
//备    注：
//===================================================
using UnityEngine;
using System.Collections;

public sealed class SecurityUtil
{
    #region xorScale 异或因子
    /// <summary>
    /// 异或因子
    /// </summary>
    private static readonly byte[] xorScale = new byte[] { 45, 66, 38, 55, 23, 254, 9, 165, 90, 19, 41, 45, 201, 58, 55, 37, 254, 185, 165, 169, 19, 171 };//.data文件的xor加解密因子
    #endregion

    private SecurityUtil()
    {

    }

    /// <summary>
    /// 对数组进行异或
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] Xor(byte[] buffer)
    {
        //------------------
        //第3步：xor解密
        //------------------
        int iScaleLen = xorScale.Length;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(buffer[i] ^ xorScale[i % iScaleLen]);
        }
        return buffer;
    }
}
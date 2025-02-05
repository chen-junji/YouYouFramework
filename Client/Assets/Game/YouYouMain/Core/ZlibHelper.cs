using System.Collections;
using System.IO;
using System;
using zlib;

/// <summary>
/// 压缩帮助类
/// </summary>
public class ZlibHelper
{
    #region CompressBytes 对原始字节数组进行zlib压缩，得到处理结果字节数组
    /// <summary>
    /// 对原始字节数组进行zlib压缩，得到处理结果字节数组
    /// </summary>
    /// <param name="OrgByte">需要被压缩的原始Byte数组数据</param>
    /// <param name="CompressRate">压缩率：默认为zlibConst.Z_DEFAULT_COMPRESSION</param>
    /// <returns>压缩后的字节数组，如果出错则返回null</returns>
    public static byte[] CompressBytes(byte[] OrgByte, int CompressRate = zlibConst.Z_BEST_SPEED)
    {
        if (OrgByte == null) return null;

        using (MemoryStream OrgStream = new MemoryStream(OrgByte))
        {
            using (MemoryStream CompressedStream = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(CompressedStream, CompressRate))
                {
                    try
                    {
                        CopyStream(OrgStream, outZStream);
                        outZStream.finish();//重要！否则结果数据不完整！
                                            //程序执行到这里，CompressedStream就是压缩后的数据
                        if (CompressedStream == null) return null;

                        return CompressedStream.ToArray();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
    #endregion

    #region DeCompressBytes 对经过zlib压缩的数据，进行解密和zlib解压缩，得到原始字节数组
    /// <summary>
    /// 对经过zlib压缩的数据，进行解密和zlib解压缩，得到原始字节数组
    /// </summary>
    /// <param name="CompressedBytes">被压缩的Byte数组数据</param>
    /// <returns>解压缩后的字节数组，如果出错则返回null</returns>
    public static byte[] DeCompressBytes(byte[] CompressedBytes)
    {
        if (CompressedBytes == null) return null;

        using (MemoryStream CompressedStream = new MemoryStream(CompressedBytes))
        {
            using (MemoryStream OrgStream = new MemoryStream())
            {
                using (ZOutputStream outZStream = new ZOutputStream(OrgStream))
                {
                    try
                    {
                        //-----------------------
                        //解压缩
                        //-----------------------
                        CopyStream(CompressedStream, outZStream);
                        outZStream.finish();//重要！
                                            //程序执行到这里，OrgStream就是解压缩后的数据

                        if (OrgStream == null)
                        {
                            return null;
                        }
                        return OrgStream.ToArray();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }
    }
    #endregion

    #region CompressString 压缩字符串
    /// <summary>
    /// 压缩字符串
    /// </summary>
    /// <param name="SourceString">需要被压缩的字符串</param>
    /// <returns>压缩后的字符串，如果失败则返回null</returns>
    public static string CompressString(string SourceString, int CompressRate = zlibConst.Z_DEFAULT_COMPRESSION)
    {
        byte[] byteSource = System.Text.Encoding.UTF8.GetBytes(SourceString);
        byte[] byteCompress = CompressBytes(byteSource, CompressRate);
        if (byteCompress != null)
        {
            return Convert.ToBase64String(byteCompress);
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region DecompressString 解压字符串
    /// <summary>
    /// 解压字符串
    /// </summary>
    /// <param name="SourceString">需要被解压的字符串</param>
    /// <returns>解压后的字符串，如果处所则返回null</returns>
    public static string DecompressString(string SourceString)
    {
        byte[] byteSource = Convert.FromBase64String(SourceString);
        byte[] byteDecompress = DeCompressBytes(byteSource);
        if (byteDecompress != null)
        {

            return System.Text.Encoding.UTF8.GetString(byteDecompress);
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region CopyStream 拷贝流
    /// <summary>
    /// 拷贝流
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    private static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[2000];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0)
        {
            output.Write(buffer, 0, len);
        }
        output.Flush();
    }
    #endregion

    #region GetStringByGZIPData 将解压缩过的二进制数据转换回字符串
    /// <summary>
    /// 将解压缩过的二进制数据转换回字符串
    /// </summary>
    /// <param name="zipData"></param>
    /// <returns></returns>
    public static string GetStringByGZIPData(byte[] zipData)
    {
        return (string)(System.Text.Encoding.UTF8.GetString(zipData));
    }
    #endregion
}
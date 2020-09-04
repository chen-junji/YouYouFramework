using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class MMO_MemoryStream : MemoryStream
{
    public MMO_MemoryStream()
    {

    }

    public MMO_MemoryStream(byte[] buffer) : base(buffer)
    {

    }

    #region Short
    /// <summary>
    /// 从流中读取一个short数据
    /// </summary>
    /// <returns></returns>
    public short ReadShort()
    {
        byte[] arr = new byte[2];
        base.Read(arr, 0, 2);
        return BitConverter.ToInt16(arr, 0);
    }

    /// <summary>
    /// 把一个short数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteShort(short value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region UShort
    /// <summary>
    /// 从流中读取一个ushort数据
    /// </summary>
    /// <returns></returns>
    public ushort ReadUShort()
    {
        byte[] arr = new byte[2];
        base.Read(arr, 0, 2);
        return BitConverter.ToUInt16(arr, 0);
    }

    /// <summary>
    /// 把一个ushort数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteUShort(ushort value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region Int
    /// <summary>
    /// 从流中读取一个int数据
    /// </summary>
    /// <returns></returns>
    public int ReadInt()
    {
        byte[] arr = new byte[4];
        base.Read(arr, 0, 4);
        return BitConverter.ToInt32(arr, 0);
    }

    /// <summary>
    /// 把一个int数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteInt(int value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region UInt
    /// <summary>
    /// 从流中读取一个uint数据
    /// </summary>
    /// <returns></returns>
    public uint ReadUInt()
    {
        byte[] arr = new byte[4];
        base.Read(arr, 0, 4);
        return BitConverter.ToUInt32(arr, 0);
    }

    /// <summary>
    /// 把一个uint数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteUInt(uint value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region Long
    /// <summary>
    /// 从流中读取一个long数据
    /// </summary>
    /// <returns></returns>
    public long ReadLong()
    {
        byte[] arr = new byte[8];
        base.Read(arr, 0, 8);
        return BitConverter.ToInt64(arr, 0);
    }

    /// <summary>
    /// 把一个long数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteLong(long value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region ULong
    /// <summary>
    /// 从流中读取一个ulong数据
    /// </summary>
    /// <returns></returns>
    public ulong ReadULong()
    {
        byte[] arr = new byte[4];
        base.Read(arr, 0, 4);
        return BitConverter.ToUInt64(arr, 0);
    }

    /// <summary>
    /// 把一个ulong数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteULong(ulong value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region Float
    /// <summary>
    /// 从流中读取一个float数据
    /// </summary>
    /// <returns></returns>
    public float ReadFloat()
    {
        byte[] arr = new byte[4];
        base.Read(arr, 0, 4);
        return BitConverter.ToSingle(arr, 0);
    }

    /// <summary>
    /// 把一个float数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteFloat(float value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region Double
    /// <summary>
    /// 从流中读取一个double数据
    /// </summary>
    /// <returns></returns>
    public double ReadDouble()
    {
        byte[] arr = new byte[8];
        base.Read(arr, 0, 8);
        return BitConverter.ToDouble(arr, 0);
    }

    /// <summary>
    /// 把一个double数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteDouble(double value)
    {
        byte[] arr = BitConverter.GetBytes(value);
        base.Write(arr, 0, arr.Length);
    }
    #endregion

    #region Bool
    /// <summary>
    /// 从流中读取一个bool数据
    /// </summary>
    /// <returns></returns>
    public bool ReadBool()
    {
        return base.ReadByte() == 1;
    }

    /// <summary>
    /// 把一个bool数据写入流
    /// </summary>
    /// <param name="value"></param>
    public void WriteBool(bool value)
    {
        base.WriteByte((byte)(value == true ? 1 : 0));
    }
    #endregion

    #region UTF8String
    /// <summary>
    /// 从流中读取一个sting数组
    /// </summary>
    /// <returns></returns>
    public string ReadUTF8String()
    {
        ushort len = this.ReadUShort();
        byte[] arr = new byte[len];
        base.Read(arr, 0, len);
        return Encoding.UTF8.GetString(arr);
    }

    /// <summary>
    /// 把一个string数据写入流
    /// </summary>
    /// <param name="str"></param>
    public void WriteUTF8String(string str)
    {
        if (str == null) str = string.Empty;

        byte[] arr = Encoding.UTF8.GetBytes(str);
        if (arr.Length > 65535)
        {
            throw new InvalidCastException("字符串超出范围");
        }
        WriteUShort((ushort)arr.Length);
        base.Write(arr, 0, arr.Length);
    }
    #endregion
}

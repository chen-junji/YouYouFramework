using System;
using System.Text;
using System.Collections.Generic;

public class StringHelper
{
    static private int sbnum = 0;
    static public string[] s_StringParamBuffer = new string[20];
    static private Queue<StringBuilder> s_StringBuilderPool = new Queue<StringBuilder>();
    private static StringFastBuilder sfb = new StringFastBuilder(32);

    public static string IntToStr(int i)
    {
        sfb.Clear();
        sfb.Append(i);
        return sfb.ToString();
    }

    public static string UlongToStr(ulong i)
    {
        sfb.Clear();
        sfb.Append(i);
        return sfb.ToString();
    }

    public static string FloatToStr(float i)
    {
        sfb.Clear();
        sfb.Append(i);
        return sfb.ToString();
    }

    public static string PoolRet(ref StringBuilder sb)
    {
        string ret = sb.ToString();
        PoolDel(ref sb);
        return ret;
    }

    public static StringBuilder PoolNew()
    {
        if (s_StringBuilderPool.Count > 0)
        {
            StringBuilder sb = s_StringBuilderPool.Dequeue();
            sb.Clear();
            sbnum++;
            return sb;
        }
        else
        {
            StringBuilder sb = new StringBuilder(256);
            sbnum++;
            return sb;
        }
    }

    public static StringBuilder PoolNew(string InitString)
    {
        if (s_StringBuilderPool.Count > 0)
        {
            StringBuilder sb = s_StringBuilderPool.Dequeue();
            sb.Clear();
            sb.Append(InitString);
            sbnum++;
            return sb;
        }
        else
        {
            StringBuilder sb = new StringBuilder(InitString, 256);
            sbnum++;
            return sb;
        }
    }

    public static void PoolDel(ref StringBuilder sb)
    {
        if (sb != null)
        {
            if (s_StringBuilderPool.Count < 100)
            {
                s_StringBuilderPool.Enqueue(sb);
            }
            sb = null;
            sbnum--;
        }
    }
}

/// <summary>
/// stringBuilder方法拓展类
/// 由于方法会用到一个全局静态变量，因此该类中的提供的方法目前只允许在主线程中使用
/// </summary>
public static class StringBuilderExtension
{
    //字符数组缓存，insertNoGC方法用
    private static char[] bufferCharArray = new char[256];

    //bufferCharArray的自动扩容最大上限，未超过上限则双倍扩容，否则根据实际长度扩容
    private const int MaxBufferCharLen = 1024;

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public static StringBuilder AppendStringBuilderNoGC(this StringBuilder sb, StringBuilder newValue)
    {
        if (newValue.Length > 0)
        {
            int newlen = newValue.Length;
            int curlen = sb.Length;
            sb.EnsureCapacity(sb.Length + newlen);
            for (int i = 0; i < newlen; i++)
            {
                sb.InsertNoGC(newValue[i], curlen++);
            }
        }
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormat3StringSlice(this StringBuilder sb, string format, StringSlice arg0, StringSlice arg1, StringSlice arg2)
    {
        StringBuilder tmpsb = StringHelper.PoolNew();
        tmpsb.Append(format);
        tmpsb.ReplaceNoGC("{0}", arg0);
        tmpsb.ReplaceNoGC("{1}", arg1);
        tmpsb.ReplaceNoGC("{2}", arg2);
        sb.AppendStringBuilderNoGC(tmpsb);
        StringHelper.PoolDel(ref tmpsb);
        return sb;
    }

    static private string[] ParamNumArray = new string[] { "{0}", "{1}", "{2}", "{3}", "{4}", "{5}", "{6}", "{7}", "{8}", "{9}", "{10}", "{11}", "{12}", "{13}", "{14}", "{15}", "{16}", "{17}" };

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_Int(this StringBuilder sb, string format, int arg0)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], StringHelper.IntToStr((int)arg0));
        return sb;
    }
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_Int_Int(this StringBuilder sb, string format, int arg0, int arg1)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], StringHelper.IntToStr((int)arg0));
        sb = sb.ReplaceNoGC(ParamNumArray[1], StringHelper.IntToStr((int)arg1));
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_Int_String(this StringBuilder sb, string format, int arg0, String arg1)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], StringHelper.IntToStr((int)arg0));
        sb = sb.ReplaceNoGC(ParamNumArray[1], arg1);
        return sb;
    }
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_String(this StringBuilder sb, string format, String arg0)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], arg0);
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_String_Int(this StringBuilder sb, string format, String arg0, int arg1)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], arg0);
        sb = sb.ReplaceNoGC(ParamNumArray[1], StringHelper.IntToStr((int)arg1));
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC_String_String(this StringBuilder sb, string format, String arg0, String arg1)
    {
        sb.Clear();
        sb.Append(format);
        sb = sb.ReplaceNoGC(ParamNumArray[0], arg0);
        sb = sb.ReplaceNoGC(ParamNumArray[1], arg1);
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGC(this StringBuilder sb, string format, params object[] args)
    {
        int argslen = args.Length;
        int reallen = 0;
        sb.Clear();
        sb.Append(format);

        for (int i = 0; i < argslen; i++)
        {
            if (args[i] != null)
            {
                Type t = args[i].GetType();

                if (t.IsValueType)
                {
                    if (t == typeof(double))
                    {
                        double d = (double)args[i];
                        StringHelper.s_StringParamBuffer[i] = StringHelper.IntToStr((int)d);
                    }
                    else if (t == typeof(int))
                    {
                        StringHelper.s_StringParamBuffer[i] = StringHelper.IntToStr((int)args[i]);
                    }
                    else if (t == typeof(ulong))
                    {
                        StringHelper.s_StringParamBuffer[i] = StringHelper.UlongToStr((ulong)args[i]);
                    }
                    else if (t == typeof(float))
                    {
                        StringHelper.s_StringParamBuffer[i] = StringHelper.FloatToStr((float)args[i]);
                    }
                    else
                    {
                        StringHelper.s_StringParamBuffer[i] = args[i].ToString();
                    }
                }
                else
                {
                    StringHelper.s_StringParamBuffer[i] = args[i].ToString();
                }
                reallen++;
            }
        }
        for (int j = 0; j < reallen; j++)
        {
            if (StringHelper.s_StringParamBuffer[j] != null)
            {
                if (j < ParamNumArray.Length)
                {
                    sb.ReplaceNoGC(ParamNumArray[j], StringHelper.s_StringParamBuffer[j]);
                }
                else
                {
                    sb.ReplaceNoGC("{" + j.ToString() + "}", StringHelper.s_StringParamBuffer[j]);
                }
            }
        }
        //sb.AppendFormat(format, StringHelper.s_StringParamBuffer);
        for (int i = 0; i < argslen; i++)
        {
            StringHelper.s_StringParamBuffer[i] = null;
        }
        return sb;
    }


    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder AppendFormatNoGCStringSlice(this StringBuilder sb, string format, List<StringSlice> lss)
    {
        int argslen = lss.Count;
        sb.Clear();
        sb.Append(format);

        for (int i = 0; i < argslen; i++)
        {
            if (i < ParamNumArray.Length)
            {
                sb.ReplaceNoGC(ParamNumArray[i], lss[i]);
            }
            else
            {
                sb.ReplaceNoGC("{" + i.ToString() + "}", lss[i]);
            }
        }
        return sb;
    }


    /*
     *
    string str = "The quick br!wn d#g jumps #ver the lazy cat.";
    System.Text.StringBuilder sb = new System.Text.StringBuilder(str);

    sb.ReplaceNoGC("#", "!", 15, 29);        // Some '#' -> '!'
    sb.ReplaceNoGC("!", "o");                // All '!' -> 'o'
    sb.ReplaceNoGC("cat", "dog");            // All "cat" -> "dog"
    sb.ReplaceNoGC("dog", "fox", 15, 20);    // Some "dog" -> "fox"

     */
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, string oldValue, string newValue, int startIndex, int count)
    {
        if (sb.Length == 0 || string.IsNullOrEmpty(oldValue) || count == 0)
        {
            return sb;
        }

        int startidx = sb.IndexOf(oldValue, startIndex);
        while (startidx >= 0 && startidx + oldValue.Length <= startIndex + count)
        {
            int newstartidx = startidx;
            sb.Remove(startidx, oldValue.Length);
            sb.InsertNoGC(newValue, startidx);
            newstartidx += newValue.Length;
            startidx = sb.IndexOf(oldValue, newstartidx);
        }
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, string oldValue, StringBuilder newValue, int startIndex, int count)
    {
        if (sb.Length == 0 || string.IsNullOrEmpty(oldValue) || count == 0)
        {
            return sb;
        }

        int startidx = sb.IndexOf(oldValue, startIndex);
        while (startidx >= 0 && startidx + oldValue.Length <= startIndex + count)
        {
            int newstartidx = startidx;
            sb.Remove(startidx, oldValue.Length);
            int newidx = startidx;
            for (int i = 0; i < newValue.Length; i++)
            {
                sb.InsertNoGC(newValue[i], newidx++);
            }

            newstartidx += newValue.Length;
            startidx = sb.IndexOf(oldValue, newstartidx);
        }
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, string oldValue, StringSlice newValue)
    {
        if (sb.Length == 0 || string.IsNullOrEmpty(oldValue))
        {
            return sb;
        }
        int startidx = sb.IndexOf(oldValue);
        while (startidx >= 0)
        {
            sb.Remove(startidx, oldValue.Length);
            {
                int newlen = newValue.Length;
                int sidx = startidx;
                for (int i = 0; i < newlen; i++)
                {
                    sb.InsertNoGC(newValue[i], sidx++);
                }
                startidx += newlen;
            }
            startidx = sb.IndexOf(oldValue, startidx);
        }
        return sb;
    }


    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, string oldValue, string newValue)
    {
        if (sb.Length == 0 || string.IsNullOrEmpty(oldValue))
        {
            return sb;
        }
        int startidx = sb.IndexOf(oldValue);
        while (startidx >= 0)
        {
            sb.Remove(startidx, oldValue.Length);
            sb.InsertNoGC(newValue, startidx);
            startidx += newValue.Length;
            startidx = sb.IndexOf(oldValue, startidx);
        }
        return sb;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, string oldValue, StringBuilder newValue)
    {
        if (sb.Length == 0 || string.IsNullOrEmpty(oldValue))
        {
            return sb;
        }
        int startidx = sb.IndexOf(oldValue);
        while (startidx >= 0)
        {
            sb.Remove(startidx, oldValue.Length);
            int newidx = startidx;
            for (int i = 0; i < newValue.Length; i++)
            {
                sb.InsertNoGC(newValue[i], newidx++);
            }
            startidx += newValue.Length;
            startidx = sb.IndexOf(oldValue, startidx);
        }
        return sb;
    }
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder ReplaceNoGC(this StringBuilder sb, StringBuilder oldValue, string newValue, int startIndex, int count)
    {
        if (sb.Length == 0 || null == oldValue || count == 0)
        {
            return sb;
        }

        int startidx = sb.IndexOf(oldValue, startIndex);
        while (startidx >= 0 && startidx + oldValue.Length <= startIndex + count)
        {
            int newstartidx = startidx;
            sb.Remove(startidx, oldValue.Length);
            sb.InsertNoGC(newValue, startidx);
            newstartidx += newValue.Length;
            startidx = sb.IndexOf(oldValue, newstartidx);
        }
        return sb;
    }
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static StringBuilder SubStringBuilder(this StringBuilder sb, StringBuilder from, int startIndex, int count)
    {
        if (null == from)
        {
            return sb;
        }
        try
        {
            if (startIndex >= 0 && startIndex + count <= from.Length)
            {
                for (int i = 0; i < count; i++)
                {
                    sb.InsertNoGC(from[startIndex + i], i);
                }
            }
        }
        catch (System.Exception)
        {

        }

        return sb;
    }
    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static int LastIndexOf(this StringBuilder sb, string value)
    {
        if (string.IsNullOrEmpty(value))
            return -1;
        int index;
        int length = value.Length;
        int sblen = sb.Length;
        int maxSearchLength = (sb.Length - length) + 1;

        for (int i = 0; i < maxSearchLength; ++i)
        {
            if (sb[sblen - 1 - i] == value[length - 1])
            {
                index = 1;
                while ((index < length) && (sb[sblen - 1 - i - index] == value[length - 1 - index]))
                    ++index;

                if (index == length)
                    return sblen - 1 - i - length + 1;
            }
        }
        return -1;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static int LastIndexOf(this StringBuilder sb, char ch)
    {
        int sblen = sb.Length;

        for (int i = 0; i < sblen; ++i)
        {
            if (sb[sblen - 1 - i] == ch)
            {
                return sblen - 1 - i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Returns the index of the start of the contents in a StringBuilder
    /// 仅允许在主线程中使用
    /// </summary>        
    /// <param name="value">The string to find</param>
    /// <param name="startIndex">The starting index.</param>
    /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
    /// <returns></returns>
    public static int IndexOf(this StringBuilder sb, string value, int startIndex = 0, bool ignoreCase = false)
    {
        int index;
        int length = value.Length;
        int maxSearchLength = (sb.Length - length) + 1;

        if (ignoreCase)
        {
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                {
                    index = 1;
                    while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        for (int i = startIndex; i < maxSearchLength; ++i)
        {
            if (sb[i] == value[0])
            {
                index = 1;
                while ((index < length) && (sb[i + index] == value[index]))
                    ++index;

                if (index == length)
                    return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static int IndexOf(this StringBuilder sb, char value, int startIndex = 0, bool ignoreCase = false)
    {
        int maxSearchLength = sb.Length;

        if (ignoreCase)
        {
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (Char.ToLower(sb[i]) == Char.ToLower(value))
                {
                    return i;
                }
            }
            return -1;
        }

        for (int i = startIndex; i < maxSearchLength; ++i)
        {
            if (sb[i] == value)
            {
                return i;
            }
        }
        return -1;
    }


    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static int IndexOf(this StringBuilder sb, StringBuilder value, int startIndex = 0, bool ignoreCase = false)
    {
        int index;
        int length = value.Length;
        int maxSearchLength = (sb.Length - length) + 1;

        if (ignoreCase)
        {
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                {
                    index = 1;
                    while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        for (int i = startIndex; i < maxSearchLength; ++i)
        {
            if (sb[i] == value[0])
            {
                index = 1;
                while ((index < length) && (sb[i + index] == value[index]))
                    ++index;

                if (index == length)
                    return i;
            }
        }

        return -1;
    }

    #region ======================Insert========================

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static void InsertNoGC(this StringBuilder sb, string from, int startIndex, int count = -1)
    {
        if (count < 0)
        {
            count = from.Length;
        }
        if (from == null || from.Length < count || sb == null)
        {
            //Log.Error("To StringBuilder or From String is invalid!!");
            return;
        }
        if (startIndex > sb.Length)
        {
            //Log.Error("startIndex is larger StringBuilder length!!");
            return;
        }

        int originLen = sb.Length;
        if (sb.Capacity < sb.Length + count)
        {
            //Log.Warning("This stringbuilder length is larger then capacity, and may cost GC when clear!!");
        }

        if (originLen - startIndex + count > bufferCharArray.Length)
        {
            bufferCharArray =
                new char[Math.Max(
                    bufferCharArray.Length * 2 > MaxBufferCharLen ? MaxBufferCharLen : bufferCharArray.Length * 2,
                    originLen - startIndex + count)];
            //Log.Warning("BufferCharArray has not enough capacity to buff This stringbuilder, and will cost GC to create a larger one!!");
        }

        from.CopyTo(0, bufferCharArray, 0, count);
        sb.CopyTo(startIndex, bufferCharArray, count, originLen - startIndex);
        sb.Remove(startIndex, sb.Length - startIndex);
        sb.Append(bufferCharArray, 0, originLen - startIndex + count);
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static void InsertNoGC(this StringBuilder sb, StringBuilder from, int startIndex, int count = -1)
    {
        if (count < 0)
        {
            count = from.Length;
        }
        if (from == null || from.Length < count || sb == null)
        {
            //Log.Error("To StringBuilder or From String is invalid!!");
            return;
        }
        if (startIndex > sb.Length)
        {
            //Log.Error("startIndex is larger StringBuilder length!!");
            return;
        }

        int originLen = sb.Length;
        if (sb.Capacity < sb.Length + count)
        {
            //Log.Warning("This stringbuilder length will larger then capacity, and may cost GC when clear!!");
        }

        if (originLen - startIndex + count > bufferCharArray.Length)
        {
            bufferCharArray =
                new char[Math.Max(
                    bufferCharArray.Length * 2 > MaxBufferCharLen ? MaxBufferCharLen : bufferCharArray.Length * 2,
                    originLen - startIndex + count)];
            //Log.Warning("BufferCharArray has not enough capacity to buff This stringbuilder, and will cost GC to create a larger one!!");
        }

        from.CopyTo(0, bufferCharArray, 0, count);
        sb.CopyTo(startIndex, bufferCharArray, count, originLen - startIndex);
        sb.Remove(startIndex, sb.Length - startIndex);
        sb.Append(bufferCharArray, 0, originLen - startIndex + count);
    }

    /// <summary>
    /// 仅允许在主线程中使用
    /// </summary>
    public static void InsertNoGC(this StringBuilder sb, char from, int startIndex)
    {
        if (sb == null)
        {
            //Log.Error("To StringBuilder is invalid!!");
            return;
        }
        if (startIndex > sb.Length)
        {
            //Log.Error("startIndex is larger StringBuilder length!!");
            return;
        }

        int originLen = sb.Length;

        if (sb.Capacity < sb.Length + 1)
        {
            //Log.Warning("This stringbuilder length will larger then capacity, and may cost GC when clear!!");
        }

        if (originLen - startIndex + 1 > bufferCharArray.Length)
        {
            bufferCharArray =
                new char[Math.Max(
                    bufferCharArray.Length * 2 > MaxBufferCharLen ? MaxBufferCharLen : bufferCharArray.Length * 2,
                    originLen - startIndex + 1)];
            //Log.Warning("BufferCharArray has not enough capacity to buff This stringbuilder, and will cost GC to create a larger one!!");
        }

        bufferCharArray[0] = from;
        sb.CopyTo(startIndex, bufferCharArray, 1, originLen - startIndex);
        sb.Remove(startIndex, sb.Length - startIndex);
        sb.Append(bufferCharArray, 0, originLen - startIndex + 1);
    }

    #endregion

}
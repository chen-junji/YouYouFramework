using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class JsonHelper
{
    /// <summary>
    /// 对象序列化成JSON字符串。
    /// </summary>
    /// <param name="obj">序列化对象</param>
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// JSON字符串序列化成对象。
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="json">JSON字符串</param>
    /// <returns></returns>
    public static T ToObject<T>(this string json)
    {
        return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// JSON字符串序列化成集合。
    /// </summary>
    /// <typeparam name="T">集合类型</typeparam>
    /// <param name="json">JSON字符串</param>
    /// <returns></returns>
    public static List<T> ToList<T>(this string json)
    {
        return ToObject<List<T>>(json);
    }

    /// <summary>
    /// JSON字符串序列化成DataTable。
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns></returns>
    public static DataTable ToTable(this string json)
    {
        return json == null ? null : JsonConvert.DeserializeObject<DataTable>(json);
    }

    /// <summary>
    /// 按属性名截取Json
    /// </summary>
    /// <param name="json">原Json字符串</param>
    /// <param name="attrArray">截取标识属性名</param>
    /// <returns></returns>
    public static string JsonCutApart(this string json, params string[] attrArray)
    {
        JToken root = JToken.Parse(json);
        foreach (var item in attrArray)
        {
            if (string.IsNullOrEmpty(item)) continue;
            root = root[item];
        }
        return Convert.ToString(root);
    }
    public static T JsonCutApart<T>(this string json, params string[] attrArray)
    {
        return JsonCutApart(json, attrArray).ToObject<T>();
    }
}
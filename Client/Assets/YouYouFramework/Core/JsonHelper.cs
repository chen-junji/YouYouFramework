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
	/// <param name="ignoreProperties">设置需要忽略的属性</param>
	/// <returns></returns>
	public static string ToJson(this object obj, params string[] ignoreProperties)
	{
		JsonSerializerSettings settings = new JsonSerializerSettings();
		settings.Formatting = Formatting.Indented;
		settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
		settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		settings.ContractResolver = new JsonPropertyContractResolver(ignoreProperties);
		return JsonConvert.SerializeObject(obj, settings);
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

/// <summary>
/// JSON分解器-设置。
/// </summary>
public class JsonPropertyContractResolver : DefaultContractResolver
{
	/// <summary>
	/// 需要排除的属性。
	/// </summary>
	private IEnumerable<string> _listExclude;

	public JsonPropertyContractResolver(params string[] ignoreProperties)
	{
		this._listExclude = ignoreProperties;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		//设置需要输出的属性。
		return base.CreateProperties(type, memberSerialization).ToList().FindAll(p => !_listExclude.Contains(p.PropertyName));
	}
}

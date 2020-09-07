using System.Collections;
using System.Collections.Generic;
using YouYouServer.Common;
using YouYouServer.Core;

/// <summary>
/// 数据表管理基类
/// </summary>
/// <typeparam name="T">数据表管理子类的类型</typeparam>
/// <typeparam name="P">数据表实体子类的类型</typeparam>
public abstract class DataTableDBModelBase<T, P>
where T : class, new()
where P : DataTableEntityBase
{
	/// <summary>
	/// Entity对象的集合
	/// </summary>
	protected List<P> m_List;

	/// <summary>
	/// Key:Entity的ID
	/// Value:Entity对象
	/// </summary>
	protected Dictionary<int, P> m_Dic;

	public DataTableDBModelBase()
	{
		m_List = new List<P>();
		m_Dic = new Dictionary<int, P>();
	}

	#region 需要子类实现的属性,方法
	/// <summary>
	/// 数据表名称
	/// </summary>
	public abstract string DataTableName { get; }
	/// <summary>
	/// 加载数据列表
	/// </summary>
	protected abstract void LoadList(MMO_MemoryStream ms);
	#endregion

	#region LoadData 加载数据表数据
	/// <summary>
	/// 加载数据表数据
	/// </summary>
	public void LoadData()
	{
		//1.拿到这个表格的buffer
		byte[] buffer = YFIOUtil.GetBuffer(string.Format(ServerConfig.DataTablePath + "/{0}.bytes", DataTableName), false);
		using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
		{
			LoadList(ms);
		}
	}
	#endregion

	#region GetList 获取子类对应的数据实体List
	/// <summary>
	/// 获取子类对应的数据实体List
	/// </summary>
	/// <returns></returns>
	public List<P> GetList()
	{
		return m_List;
	}
	#endregion

	#region GetDic 根据ID获取实体
	/// <summary>
	/// 根据ID获取实体
	/// </summary>
	public P GetDic(int id)
	{
		P p;
		if (m_Dic.TryGetValue(id, out p))
		{
			return p;
		}
		else
		{
			//Debug.Log("该ID对应的数据实体不存在");
			return null;
		}
	}
	#endregion

	/// <summary>
	/// 清空数据
	/// </summary>
	public void Clear()
	{
		m_List.Clear();
		m_Dic.Clear();
	}

}

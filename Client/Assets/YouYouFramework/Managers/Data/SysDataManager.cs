using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class SysDataManager : IDisposable
{
	public long CurrServerTime
	{
		get
		{
			if (CurrChannelConfig == null) return (long)Time.unscaledTime;
			return CurrChannelConfig.ServerTime + (long)Time.unscaledTime;
		}
	}

	public ChannelConfigEntity CurrChannelConfig { get; private set; }

	public SysDataManager()
	{
		CurrChannelConfig = new ChannelConfigEntity();
	}
	/// <summary>
	/// 清空数据
	/// </summary>
	public void Clear()
	{

	}
	public void Dispose()
	{
	}

	/// <summary>
	/// 根据系统码获取提示内容
	/// </summary>
	/// <param name="sysCode"></param>
	/// <returns></returns>
	public string GetSysCodeContent(int sysCode)
	{
		Sys_CodeEntity sys_Code = GameEntry.DataTable.Sys_CodeDBModel.GetDic(sysCode);
		if (sys_Code != null)
		{
			return GameEntry.Localization.GetString(sys_Code.Name);
		}
		return string.Empty;
	}
}

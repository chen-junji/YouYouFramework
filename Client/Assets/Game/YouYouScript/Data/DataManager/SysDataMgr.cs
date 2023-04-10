using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class SysDataMgr : IDisposable
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

	public SysDataMgr()
	{
		CurrChannelConfig = new ChannelConfigEntity();
	}
	public void Dispose()
	{
	}
}

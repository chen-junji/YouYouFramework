using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// 用户数据
/// </summary>
public class UserDataManager : IDisposable
{
	/// <summary>
	/// 共享的用户数据
	/// </summary>
	public ShareUserData ShareUserData;

	public UserDataManager()
	{
		ShareUserData = new ShareUserData();
	}

	/// <summary>
	/// 清空数据
	/// </summary>
	public void Clear()
	{
		ShareUserData.Dispose();
	}

	public void Dispose()
	{
	}
}

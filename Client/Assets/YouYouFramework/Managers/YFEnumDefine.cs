using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	#region AssetCategory 资源分类
	/// <summary>
	/// 资源分类
	/// </summary>
	public enum AssetCategory
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,
		/// <summary>
		/// 声音
		/// </summary>
		Audio = 1,
		/// <summary>
		/// 自定义Shaders
		/// </summary>
		CusShaders = 2,
		/// <summary>
		/// 表格
		/// </summary>
		DataTable = 3,
		/// <summary>
		/// 特效
		/// </summary>
		Effects = 4,
		/// <summary>
		/// 角色预设
		/// </summary>
		RolePrefab = 5,
		/// <summary>
		/// 角色资源
		/// </summary>
		RoleSources = 6,
		/// <summary>
		/// 场景
		/// </summary>
		Scenes = 7,
		/// <summary>
		/// 字体
		/// </summary>
		UIFont = 8,
		/// <summary>
		/// UI预设
		/// </summary>
		UIPrefab = 9,
		/// <summary>
		/// UI资源
		/// </summary>
		UIRes = 10,
		/// <summary>
		/// lua脚本
		/// </summary>
		xLuaLogic = 11
	}
	#endregion

	/// <summary>
	/// 日志分类
	/// </summary>
	public enum LogCategory
	{
		/// <summary>
		/// 普通日志
		/// </summary>
		Normal,
		/// <summary>
		/// 流程日志
		/// </summary>
		Procedure,
		/// <summary>
		/// 资源管理日志
		/// </summary>
		Resource,
		/// <summary>
		/// 协议日志
		/// </summary>
		Proto
	}

	/// <summary>
	/// 协议分类
	/// </summary>
	public enum ProtoCategory : byte
	{
		Client2GatewayServer,
		GatewayServer2Client,
		Client2WorldServer,
		WorldServer2Client,
		Client2GameServer,
		GameServer2Client,
		GameServer2WorldServer,
		WorldServer2GameServer,
		GatewayServer2WorldServer,
		WorldServer2GatewayServer,
		GatewayServer2GameServer,
		GameServer2GatewayServer,
	}

	public enum LoadingType
	{
		/// <summary>
		/// 切换场景
		/// </summary>
		ChangeScene,
		/// <summary>
		/// 检查更新
		/// </summary>
		CheckVersion
	}

	/// <summary>
	/// 提示窗口,按钮显示方式
	/// </summary>
	public enum DialogFormType
	{
		/// <summary>
		/// 确定按钮
		/// </summary>
		Affirm,
		/// <summary>
		/// 确定,取消按钮
		/// </summary>
		Noraml
	}

	/// <summary>
	/// UI窗口的显示类型
	/// </summary>
	public enum UIFormShowMode
	{
		Normal = 0,
		/// <summary>
		/// 反切
		/// </summary>
		ReverseChange = 1,
	}

	/// <summary>
	/// 支付平台
	/// </summary>
	public enum PayPlatform
	{
		IOS,
		Ali_WX,
		Goggle,
	}
}
using UnityEngine;
using System.Collections;

namespace YouYou
{
	/// <summary>
	/// 协议接口
	/// </summary>
	public interface IProto : Google.Protobuf.IMessage
	{
		/// <summary>
		/// 协议编号
		/// </summary>
		ushort ProtoId { get; }

		/// <summary>
		/// 协议编码
		/// </summary>
		string ProtoEnName { get; }

		/// <summary>
		/// 协议分类
		/// </summary>
		ProtoCategory Category { get; }
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
}
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
}
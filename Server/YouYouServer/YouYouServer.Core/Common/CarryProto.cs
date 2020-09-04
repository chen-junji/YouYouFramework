using System;
using System.Collections.Generic;
using System.Text;
using YouYouServer.Core;

namespace YouYouServer.Core.Common
{
    /// <summary>
    /// 客户端发给网关服务器时使用的中转协议（运载别的协议使用）
    /// </summary>
    public class CarryProto
    {
        /// <summary>
        /// 协议分类
        /// </summary>
        public ProtoCategory Category = ProtoCategory.CarryProto;

        /// <summary>
        /// 玩家账号
        /// </summary>
        public long AccountId;

        /// <summary>
        /// 承运的协议号
        /// </summary>
        public ushort CarryProtoId;

        /// <summary>
        /// 承运协议分类（也就是承运别的协议的分类）
        /// </summary>
        public ProtoCategory CarryProtoCategory;

        /// <summary>
        /// 承运协议内容
        /// </summary>
        public byte[] Buffer;

        public CarryProto()
        {

        }

        public CarryProto(long accountId, ushort carryProtoCode, ProtoCategory carryProtoCategory, byte[] buffer)
        {
            AccountId = accountId;
            CarryProtoId = carryProtoCode;
            CarryProtoCategory = carryProtoCategory;
            Buffer = buffer;
        }

        public byte[] ToArray()
        {
            MMO_MemoryStream ms = new MMO_MemoryStream();
            ms.SetLength(0);
            ms.WriteLong(AccountId);
            ms.WriteUShort(CarryProtoId);
            ms.WriteByte((byte)CarryProtoCategory);
            ms.WriteInt(Buffer.Length);
            ms.Write(Buffer);
            return ms.ToArray();
        }

        public static CarryProto GetProto(byte[] buffer)
        {
            MMO_MemoryStream ms = new MMO_MemoryStream();
            CarryProto proto = new CarryProto();
            ms.SetLength(0);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            proto.AccountId = ms.ReadLong();
            proto.CarryProtoId = ms.ReadUShort();
            proto.CarryProtoCategory = (ProtoCategory)ms.ReadByte();

            int len = ms.ReadInt();
            proto.Buffer = new byte[len];
            ms.Read(proto.Buffer);

            return proto;
        }
    }
}
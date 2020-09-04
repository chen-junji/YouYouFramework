//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2020-02-18 23:03:22
//备    注：
//===================================================

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
/// Create By 悠游课堂 http://www.u3dol.com 陈俊基 13288578058
using YouYou;

/// <summary>
/// Socket协议监听
/// </summary>
public sealed class SocketProtoListener
{
    /// <summary>
    /// 添加协议监听
    /// </summary>
    public static void AddProtoListener()
    {
        GameEntry.Event.SocketEvent.AddEventListener(ProtoIdDefine.Proto_GWS2C_ReturnRegClient, GWS2C_ReturnRegClientHandler.OnHandler);
        GameEntry.Event.SocketEvent.AddEventListener(ProtoIdDefine.Proto_WS2C_ReturnCreateRole, WS2C_ReturnCreateRoleHandler.OnHandler);
    }

    /// <summary>
    /// 移除协议监听
    /// </summary>
    public static void RemoveProtoListener()
    {
        GameEntry.Event.SocketEvent.RemoveEventListener(ProtoIdDefine.Proto_GWS2C_ReturnRegClient, GWS2C_ReturnRegClientHandler.OnHandler);
        GameEntry.Event.SocketEvent.RemoveEventListener(ProtoIdDefine.Proto_WS2C_ReturnCreateRole, WS2C_ReturnCreateRoleHandler.OnHandler);
    }
}
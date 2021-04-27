/// Create By 悠游课堂 http://www.u3dol.com 陈俊基 13288578058
/// <summary>
/// 协议编号
/// </summary>
public class ProtoIdDefine
{
    /// <summary>
    /// 玩家向网关服务器注册客户端
    /// </summary>
    public const ushort Proto_C2GWS_RegClient = 10001;

    /// <summary>
    /// 玩家向服务器发送创建角色消息
    /// </summary>
    public const ushort Proto_C2WS_CreateRole = 20001;

    /// <summary>
    /// 游戏服务器注册到中心服务器
    /// </summary>
    public const ushort Proto_GS2WS_RegGameServer = 40001;

    /// <summary>
    /// 网关服务器返回注册客户端结果
    /// </summary>
    public const ushort Proto_GWS2C_ReturnRegClient = 15001;

    /// <summary>
    /// 网关服务器注册到游戏服务器
    /// </summary>
    public const ushort Proto_GWS2GS_RegGatewayServer = 60001;

    /// <summary>
    /// 网关服务器注册到中心服务器
    /// </summary>
    public const ushort Proto_GWS2WS_RegGatewayServer = 50001;

    /// <summary>
    /// 网关服务器通知中心服务器注册到游戏服完毕
    /// </summary>
    public const ushort Proto_GWS2WS_RegGameServerSuccess = 50002;

    /// <summary>
    /// 服务器返回创建角色消息
    /// </summary>
    public const ushort Proto_WS2C_ReturnCreateRole = 25001;

    /// <summary>
    /// 中心服务器通知网关服务器注册到游戏服
    /// </summary>
    public const ushort Proto_WS2GWS_ToRegGameServer = 55001;
}
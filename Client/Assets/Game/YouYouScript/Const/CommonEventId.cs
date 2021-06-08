public class CommonEventId
{
	#region 系统事件编号(系统事件编号 采用4位 1001(10表示模块 01表示编号))
	/// <summary>
	/// 数据表全部加载完毕
	/// </summary>
	public const ushort LoadDataTableComplete = 1001;

	/// <summary>
	/// 数据表单个加载完毕
	/// </summary>
	public const ushort LoadOneDataTableComplete = 1002;

	/// <summary>
	/// 加载Lua表格完毕
	/// </summary>
	public const ushort LoadLuaDataTableComplete = 1003;

	/// <summary>
	/// ILRuntime程序集加载完毕
	/// </summary>
	public const ushort LoadILRuntimeComplete = 1004;

	/// <summary>
	/// 加载进度条更新
	/// </summary>
	public const ushort LoadingProgressChange = 1101;
	/// <summary>
	/// 关闭"转圈圈"
	/// </summary>
	public const ushort CloseUICircle = 1102;

	/// <summary>
	/// 检查更新_开始下载
	/// </summary>
	public const ushort CheckVersionBeginDownload = 1201;
	/// <summary>
	/// 检查更新_下载中
	/// </summary>
	public const ushort CheckVersionDownloadUpdate = 1202;
	/// <summary>
	/// 检查更新_下载完毕
	/// </summary>
	public const ushort CheckVersionDownloadComplete = 1203;

	/// <summary>
	/// 预加载_开始加载
	/// </summary>
	public const ushort PreloadBegin = 1204;
	/// <summary>
	/// 预加载_开始加载
	/// </summary>
	public const ushort PreloadUpdate = 1205;
	/// <summary>
	/// 预加载_开始加载
	/// </summary>
	public const ushort PreloadComplete = 1206;

	/// <summary>
	/// Lua内存释放
	/// </summary>
	public const ushort LuaFullGc = 1208;

	/// <summary>
	/// 主Socket连接服务器成功
	/// </summary>
	public const ushort OnConnectOKToMainSocket = 1301;
	#endregion


	//=============下面这些是可以删的============
	/// <summary>
	/// 注册成功
	/// </summary>
	public const int RegComplete = 10001;

    /// <summary>
    /// 登录成功
    /// </summary>
    public const int LogOnComplete = 10002;

}
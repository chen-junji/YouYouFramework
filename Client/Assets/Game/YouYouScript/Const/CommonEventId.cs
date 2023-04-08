namespace YouYou
{
    public class CommonEventId
    {
        #region 系统事件编号(系统事件编号 采用4位 1001(10表示模块 01表示编号))
        /// <summary>
        /// 加载进度条更新
        /// </summary>
        public const string LoadingProgressChange = "LoadingProgressChange";
        /// <summary>
        /// 关闭"转圈圈"
        /// </summary>
        public const string CloseUICircle = "CloseUICircle";

        /// <summary>
        /// 检查更新_开始下载
        /// </summary>
        public const string CheckVersionBeginDownload = "CheckVersionBeginDownload";
        /// <summary>
        /// 检查更新_下载中
        /// </summary>
        public const string CheckVersionDownloadUpdate = "CheckVersionDownloadUpdate";
        /// <summary>
        /// 检查更新_下载完毕
        /// </summary>
        public const string CheckVersionDownloadComplete = "CheckVersionDownloadComplete";

        /// <summary>
        /// 预加载_开始加载
        /// </summary>
        public const string PreloadBegin = "PreloadBegin";
        /// <summary>
        /// 预加载_开始加载
        /// </summary>
        public const string PreloadUpdate = "PreloadUpdate";
        /// <summary>
        /// 预加载_开始加载
        /// </summary>
        public const string PreloadComplete = "PreloadComplete";

        /// <summary>
        /// Lua内存释放
        /// </summary>
        public const string LuaFullGc = "LuaFullGc";

        /// <summary>
        /// 主Socket连接服务器成功
        /// </summary>
        public const string OnConnectOKToMainSocket = "OnConnectOKToMainSocket";

        //背景音乐音量
        public const string PlayerBGMVolume = "PlayerBGMVolume";
        //音效音量
        public const string PlayerAudioVolume = "PlayerAudioVolume";
        //是否震动
        public const string IsShake = "IsShake";
        //游戏暂停
        public const string GamePause = "GamePause";
        //最大帧率
        public const string FrameRate = "FrameRate";
        //屏幕分辨率
        public const string Screen = "Screen";
        //画质等级
        public const string QualityLevel = "QualityLevel";
        #endregion

    }
}
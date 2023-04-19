namespace YouYou
{
    public class CommonEventId
    {
        #region 系统事件
        //预加载_加载开始
        public const string PreloadBegin = "PreloadBegin";
        //预加载_加载进度更新
        public const string PreloadUpdate = "PreloadUpdate";
        //预加载_加载完毕
        public const string PreloadComplete = "PreloadComplete";

        //主Socket连接服务器成功
        public const string OnConnectOKToMainSocket = "OnConnectOKToMainSocket";

        //场景加载进度更新
        public const string LoadingProgressChange = "LoadingProgressChange";
        //关闭"转圈圈"
        public const string CloseUICircle = "CloseUICircle";

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

        //测试事件
        public const string TestEvent = "TestEvent";
    }
}
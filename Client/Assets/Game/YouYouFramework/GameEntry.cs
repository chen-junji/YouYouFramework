using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    public class GameEntry : MonoBehaviour
    {
        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private ParamsSettings.DeviceGrade m_CurrDeviceGrade;

        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private ParamsSettings m_ParamsSettings;

        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private YouYouLanguage m_CurrLanguage;

        [FoldoutGroup("ResourceGroup")]
        [Header("游戏物体对象池父物体")]
        public Transform PoolParent;

        [FoldoutGroup("ResourceGroup")]
        /// <summary>
        /// 游戏物体对象池分组
        /// </summary>
        [SerializeField]
        public GameObjectPoolEntity[] GameObjectPoolGroups;

        [FoldoutGroup("ResourceGroup")]
        [Header("锁定的资源包")]
        /// <summary>
        /// 锁定的资源包（不会释放）
        /// </summary>
        public string[] LockedAssetBundle;

        [FoldoutGroup("UIGroup")]
        [Header("标准分辨率宽度")]
        [SerializeField]
        public int StandardWidth = 1280;

        [FoldoutGroup("UIGroup")]
        [Header("标准分辨率高度")]
        [SerializeField]
        public int StandardHeight = 720;

        [FoldoutGroup("UIGroup")]
        [Header("UI摄像机")]
        public Camera UICamera;

        [FoldoutGroup("UIGroup")]
        [Header("根画布")]
        [SerializeField]
        public Canvas UIRootCanvas;

        [FoldoutGroup("UIGroup")]
        [Header("根画布的缩放")]
        [SerializeField]
        public CanvasScaler UIRootCanvasScaler;

        [FoldoutGroup("UIGroup")]
        [Header("UI分组")]
        [SerializeField]
        public UIGroup[] UIGroups;

        [Title("支付平台选择")]
        public PayPlatform m_PayPlatform;
        public static PayPlatform PayPaltform { get; private set; }

        #region 管理器属性
        public static LoggerManager Logger { get; private set; }
        public static EventManager Event { get; private set; }
        public static TimeManager Time { get; private set; }
        public static FsmManager Fsm { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static DataTableManager DataTable { get; private set; }
        public static SocketManager Socket { get; private set; }
        public static HttpManager Http { get; private set; }
        public static DataManager Data { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static YouYouSceneManager Scene { get; private set; }
        public static AddressableManager Resource { get; private set; }
        public static DownloadManager Download { get; private set; }
        public static UIManager UI { get; private set; }
        public static AudioManager Audio { get; private set; }
        public static InputManager YouYouInput { get; private set; }
        public static WebSocketManager WebSocket { get; private set; }
        public static TaskManager Task { get; private set; }
        public static ILRuntimeManager ILRuntime { get; private set; }
        #endregion

        #region InitManagers 初始化管理器
        /// <summary>
        /// 初始化管理器
        /// </summary>
        private static void InitManagers()
        {
            Logger = new LoggerManager();
            Event = new EventManager();
            Time = new TimeManager();
            Fsm = new FsmManager();
            Procedure = new ProcedureManager();
            DataTable = new DataTableManager();
            Socket = new SocketManager();
            Http = new HttpManager();
            Data = new DataManager();
            Localization = new LocalizationManager();
            Pool = new PoolManager();
            Scene = new YouYouSceneManager();
            Resource = new AddressableManager();
            Download = new DownloadManager();
            UI = new UIManager();
            Audio = new AudioManager();
            YouYouInput = new InputManager();
            WebSocket = new WebSocketManager();
            Task = new TaskManager();
            ILRuntime = new ILRuntimeManager();

            Logger.Init();
            Procedure.Init();
            DataTable.Init();
            Socket.Init();
            Http.Init();
            Localization.Init();
            Pool.Init();
            Scene.Init();
            Resource.Init();
            Download.Init();
            UI.Init();
            Audio.Init();
            YouYouInput.Init();
            WebSocket.Init();
            Task.Init();

            //进入第一个流程
            Procedure.ChangeState(ProcedureState.Launch);
        }
        #endregion

        /// <summary>
        /// 单例
        /// </summary>
        public static GameEntry Instance { get; private set; }

        /// <summary>
        /// 全局参数设置
        /// </summary>
        public static ParamsSettings ParamsSettings { get; private set; }

        /// <summary>
        /// 当前设备等级
        /// </summary>
        public static ParamsSettings.DeviceGrade CurrDeviceGrade { get; private set; }

        /// <summary>
        /// 当前语言（要和本地化表的语言字段 一致）
        /// </summary>
        public static YouYouLanguage CurrLanguage;

        public static CameraCtrl CameraCtrl;

        public static Action ActionOnUpdate;
        public static Action<bool> ActionOnApplicationPause;
        public static Action ActionOnApplicationQuit;
        public static Action ActionOnGameEnter;

        private void Awake()
        {
            Log(LogCategory.Procedure, "GameEntry.OnAwake()");
            Instance = this;

            //屏幕常亮
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //此处以后判断如果不是编辑器模式 要根据设备信息判断等级
            CurrDeviceGrade = m_CurrDeviceGrade;
            ParamsSettings = m_ParamsSettings;
            CurrLanguage = m_CurrLanguage;
            PayPaltform = m_PayPlatform;

            //限制游戏帧数,FPS
            Application.targetFrameRate = ParamsSettings.GetGradeParamData(YFConstDefine.targetFrameRate, CurrDeviceGrade);
        }
        private void Start()
        {
            Log(LogCategory.Procedure, "GameEntry.OnStart()");
            InitManagers();
        }

        void Update()
        {
            Time.OnUpdate();
            Procedure.OnUpdate();
            Socket.OnUpdate();
            Data.OnUpdate();
            Pool.OnUpdate();
            Scene.OnUpdate();
            Resource.OnUpdate();
            Download.OnUpdate();
            UI.OnUpdate();
            Audio.OnUpdate();
            YouYouInput.OnUpdate();
            WebSocket.OnUpdate();
            Task.OnUpdate();

            ActionOnUpdate?.Invoke();
        }
        private void OnApplicationQuit()
        {
            Logger.SyncLog();
            Logger.Dispose();
            Fsm.Dispose();
            Procedure.Dispose();
            DataTable.Dispose();
            Socket.Dispose();
            Http.Dispose();
            Data.Dispose();
            Localization.Dispose();
            Pool.Dispose();
            Scene.Dispose();
            Resource.Dispose();
            Download.Dispose();
            UI.Dispose();
            YouYouInput.Dispose();

            ActionOnApplicationQuit?.Invoke();
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause) Data.PlayerPrefs.SaveDataAll();

            ActionOnApplicationPause?.Invoke(pause);
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        public static void Log(LogCategory catetory, string message, params object[] args)
        {
            switch (catetory)
            {
                default:
                case LogCategory.Normal:
#if UNITY_EDITOR || (DEBUG_LOG_NORMAL && DEBUG_MODEL)
                    Debug.Log("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));
#endif
                    break;
                case LogCategory.Procedure:
#if UNITY_EDITOR || (DEBUG_LOG_PROCEDURE && DEBUG_MODEL)
                    Debug.Log("[youyou]" + string.Format("{0}", args.Length == 0 ? message : string.Format(message, args)));
#endif
                    break;
                case LogCategory.Resource:
#if UNITY_EDITOR || (DEBUG_LOG_RESOURCE && DEBUG_MODEL)
                    Debug.Log("[youyou]" + string.Format("{0}", args.Length == 0 ? message : string.Format(message, args)));
#endif
                    break;
                case LogCategory.Proto:
#if UNITY_EDITOR || (DEBUG_LOG_PROTO && DEBUG_MODEL)
                    Debug.Log("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));
#endif
                    break;
            }
        }
        /// <summary>
        /// 打印错误日志
        /// </summary>
        public static void LogError(string message, params object[] args)
        {
#if UNITY_EDITOR || (DEBUG_LOG_ERROR && DEBUG_MODEL)
            Debug.LogError("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));
#endif
        }
    }
}
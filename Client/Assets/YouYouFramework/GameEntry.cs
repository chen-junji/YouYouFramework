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

		#region 时间缩放
		[CustomValueDrawer("SetTimeScale")]
		public float timeScale;

#if UNITY_EDITOR
		[ButtonGroup]
		[LabelText("0")]
		private void timeScale0()
		{
			timeScale = 0;
		}

		[ButtonGroup]
		[LabelText("0.5")]
		private void timeScale05()
		{
			timeScale = 0.5f;
		}

		[ButtonGroup]
		[LabelText("1")]
		private void timeScale1()
		{
			timeScale = 1;
		}

		[ButtonGroup]
		[LabelText("2")]
		private void timeScale2()
		{
			timeScale = 2;
		}

		[ButtonGroup]
		[LabelText("3")]
		private void timeScale3()
		{
			timeScale = 3;
		}

		private float SetTimeScale(float value, GUIContent label)
		{
			float ret = UnityEditor.EditorGUILayout.Slider(label, value, 0f, 3);
			UnityEngine.Time.timeScale = ret;
			return ret;
		}
#endif
		#endregion

		[Title("支付平台选择")]
		public PayPlatform m_PayPlatform;
		public static PayPlatform PayPaltform { get; private set; }
		#region 管理器属性
		/// <summary>
		/// 日志管理器
		/// </summary>
		public static LoggerManager Logger
		{
			get;
			private set;
		}

		/// <summary>
		/// 事件管理器
		/// </summary>
		public static EventManager Event
		{
			get;
			private set;
		}

		/// <summary>
		/// 时间管理器
		/// </summary>
		public static TimeManager Time
		{
			get;
			private set;
		}

		/// <summary>
		/// 状态机管理器
		/// </summary>
		public static FsmManager Fsm
		{
			get;
			private set;
		}

		/// <summary>
		/// 流程管理器
		/// </summary>
		public static ProcedureManager Procedure
		{
			get;
			private set;
		}

		/// <summary>
		/// 数据表管理器
		/// </summary>
		public static DataTableManager DataTable
		{
			get;
			private set;
		}

		/// <summary>
		/// Socket管理器
		/// </summary>
		public static SocketManager Socket
		{
			get;
			private set;
		}

		/// <summary>
		/// Http管理器
		/// </summary>
		public static HttpManager Http
		{
			get;
			private set;
		}

		/// <summary>
		/// 数据管理器
		/// </summary>
		public static DataManager Data
		{
			get;
			private set;
		}

		/// <summary>
		/// 本地化管理器
		/// </summary>
		public static LocalizationManager Localization
		{
			get;
			private set;
		}

		/// <summary>
		/// 对象池管理器
		/// </summary>
		public static PoolManager Pool
		{
			get;
			private set;
		}

		/// <summary>
		/// 场景管理器
		/// </summary>
		public static YouYouSceneManager Scene
		{
			get;
			private set;
		}

		/// <summary>
		/// 可寻址资源管理器
		/// </summary>
		public static AddressableManager Resource
		{
			get;
			private set;
		}

		/// <summary>
		/// 下载管理器
		/// </summary>
		public static DownloadManager Download
		{
			get;
			private set;
		}

		/// <summary>
		/// UI管理器
		/// </summary>
		public static UIManager UI
		{
			get;
			private set;
		}

		/// <summary>
		/// Lua管理器
		/// </summary>
		public static LuaManager Lua
		{
			get;
			private set;
		}

		/// <summary>
		/// 声音管理器
		/// </summary>
		public static AudioManager Audio;

		public static WebSocketManager WebSocket;
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
			Lua = new LuaManager();
			Audio = new AudioManager();
			WebSocket = new WebSocketManager();

			Logger.Init();
			Event.Init();
			Time.Init();
			Fsm.Init();
			Procedure.Init();
			DataTable.Init();
			Socket.Init();
			Http.Init();
			Data.Init();
			Localization.Init();
			Pool.Init();
			Scene.Init();
			Resource.Init();
			Download.Init();
			UI.Init();
			//Lua.Init();
			Audio.Init();
			WebSocket.Init();

			//进入第一个流程
			Procedure.ChangeState(ProcedureState.Launch);
		}
		#endregion

		/// <summary>
		/// 单例
		/// </summary>
		public static GameEntry Instance;

		/// <summary>
		/// 全局参数设置
		/// </summary>
		public static ParamsSettings ParamsSettings
		{
			get;
			private set;
		}

		/// <summary>
		/// 当前设备等级
		/// </summary>
		public static ParamsSettings.DeviceGrade CurrDeviceGrade
		{
			get;
			private set;
		}

		/// <summary>
		/// 当前语言（要和本地化表的语言字段 一致）
		/// </summary>
		public static YouYouLanguage CurrLanguage
		{
			get;
			set;
		}

		private void Awake()
		{
			Instance = this;

			//此处以后判断如果不是编辑器模式 要根据设备信息判断等级
			CurrDeviceGrade = m_CurrDeviceGrade;
			ParamsSettings = m_ParamsSettings;
			CurrLanguage = m_CurrLanguage;
			PayPaltform = m_PayPlatform;
		}

		void Start()
		{
			Log(LogCategory.Procedure, "GameEntry.OnStart()");
			InitManagers();

			UnityEngine.Time.timeScale = timeScale = 1;
			Application.targetFrameRate = ParamsSettings.GetGradeParamData(YFConstDefine.targetFrameRate, CurrDeviceGrade);
		}

		void Update()
		{
			Time.OnUpdate();
			Procedure.OnUpdate();
			Socket.OnUpdate();
			Pool.OnUpdate();
			Scene.OnUpdate();
			Resource.OnUpdate();
			Download.OnUpdate();
			UI.OnUpdate();
			Audio.OnUpdate();
			WebSocket.OnUpdate();
		}

		/// <summary>
		/// 销毁
		/// </summary>
		private void OnDestroy()
		{
			Logger.SyncLog();
			Logger.Dispose();
			Event.Dispose();
			Time.Dispose();
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
			Lua.Dispose();
			Audio.Dispose();
			WebSocket.Dispose();
		}

		/// <summary>
		/// 打印日志
		/// </summary>
		/// <param name="message"></param>
		public static void Log(LogCategory catetory, string message, params object[] args)
		{
			switch (catetory)
			{
				default:
				case LogCategory.Normal:
#if DEBUG_LOG_NORMAL && DEBUG_MODEL
                    Debug.Log("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));

#endif
					break;
				case LogCategory.Procedure:
#if DEBUG_LOG_PROCEDURE && DEBUG_MODEL
                    Debug.Log("[youyou]" + string.Format("<color=#ffffff>{0}</color>", args.Length == 0 ? message : string.Format(message, args)));
#endif
					break;
				case LogCategory.Resource:
#if DEBUG_LOG_RESOURCE && DEBUG_MODEL
                    Debug.Log("[youyou]" + string.Format("<color=#ace44a>{0}</color>", args.Length == 0 ? message : string.Format(message, args)));
#endif
					break;
				case LogCategory.Proto:
#if DEBUG_LOG_PROTO && DEBUG_MODEL
                    Debug.Log("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));
#endif
					break;
			}
		}

		/// <summary>
		/// 打印错误日志
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public static void LogError(string message, params object[] args)
		{
#if DEBUG_LOG_ERROR && DEBUG_MODEL
			Debug.LogError("[youyou]" + (args.Length == 0 ? message : string.Format(message, args)));
#endif
		}
	}
}
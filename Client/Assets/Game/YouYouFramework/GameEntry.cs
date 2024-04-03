using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace YouYouFramework
{
    public class GameEntry : MonoBehaviour
    {
        [FoldoutGroup("UI框架相关")]
        [Header("UI摄像机")]
        public Camera UICamera;

        [FoldoutGroup("UI框架相关")]
        [Header("根画布")]
        public Canvas UIRootCanvas;

        [FoldoutGroup("UI框架相关")]
        [Header("根画布的缩放")]
        public CanvasScaler UIRootCanvasScaler;
        public RectTransform UIRootRectTransform { get; private set; }

        [FoldoutGroup("UI框架相关")]
        [Header("UI分组")]
        public UIGroup[] UIGroups;

        [Header("游戏物体对象池分组")]
        public GameObjectPoolEntity[] GameObjectPoolGroups;

        [Header("对象池锁定的资源包")]
        public string[] LockedAssetBundle;

        [Header("声音主混合器")]
        public AudioMixer MonsterMixer;

        [Header("当前语言（要和本地化表的语言字段 一致）")]
        [SerializeField]
        private YouYouLanguage m_CurrLanguage;
        public static YouYouLanguage CurrLanguage;


        //管理器属性
        public static LoggerManager Logger { get; private set; }
        public static EventManager Event { get; private set; }
        public static TimeManager Time { get; private set; }
        public static FsmManager Fsm { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static DataTableManager DataTable { get; private set; }
        public static ModelManager Model { get; private set; }
        public static PlayerPrefsManager PlayerPrefs { get; private set; }
        public static HttpManager Http { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static YouYouSceneManager Scene { get; private set; }
        public static LoaderManager Loader { get; private set; }
        public static UIManager UI { get; private set; }
        public static AudioManager Audio { get; private set; }
        public static InputManager Input { get; private set; }
        public static TaskManager Task { get; private set; }

        public static GameEntry Instance { get; private set; }
        private void Awake()
        {
            Log(LogCategory.Procedure, "GameEntry.OnAwake()");
            Instance = this;
            CurrLanguage = m_CurrLanguage;
            UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();

            //屏幕常亮
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        private void Start()
        {
            Log(LogCategory.Procedure, "GameEntry.OnStart()");

            Logger = new LoggerManager();
            Event = new EventManager();
            Time = new TimeManager();
            Fsm = new FsmManager();
            Procedure = new ProcedureManager();
            DataTable = new DataTableManager();
            Model = new ModelManager();
            PlayerPrefs = new PlayerPrefsManager();
            Http = new HttpManager();
            Localization = new LocalizationManager();
            Pool = new PoolManager();
            Scene = new YouYouSceneManager();
            Loader = new LoaderManager();
            UI = new UIManager();
            Audio = new AudioManager();
            Input = new InputManager();
            Task = new TaskManager();

            Logger.Init();
            Procedure.Init();
            DataTable.Init();
            PlayerPrefs.Init();
            Http.Init();
            Pool.Init();
            Scene.Init();
            Loader.Init();
            UI.Init();
            Audio.Init();
            Task.Init();

            //进入第一个流程
            Procedure.ChangeState(ProcedureState.Launch);
        }

        void Update()
        {
            Time.OnUpdate();
            Procedure.OnUpdate();
            Pool.OnUpdate();
            Scene.OnUpdate();
            Loader.OnUpdate();
            UI.OnUpdate();
            Audio.OnUpdate();
            Input.OnUpdate();
            Task.OnUpdate();

            GameEntry.Event.Dispatch(CommonEventId.GameEntryOnUpdate);
        }
        private void OnApplicationQuit()
        {
            PlayerPrefs.SaveDataAll();
            Logger.SyncLog();

            GameEntry.Event.Dispatch(CommonEventId.GameEntryOnApplicationQuit);
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                PlayerPrefs.SaveDataAll();
                GameEntry.Event.Dispatch(CommonEventId.GameEntryOnApplicationPause);
            }
        }


        public static void Log(LogCategory catetory, params object[] args)
        {
#if DEBUG_LOG_NORMAL
            //由于性能原因，如果在Build Settings中没有勾上“Development Build”
            //即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            if (!Debug.isDebugBuild)
            {
                return;
            }
            Debug.Log("GameEntryLog - " + catetory.ToString() + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
        }

        public static void LogWarning(LogCategory catetory, params object[] args)
        {
#if DEBUG_LOG_WARNING
            Debug.LogWarning("GameEntryLog - " + catetory.ToString() + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
        }

        public static void LogError(LogCategory catetory, params object[] args)
        {
#if DEBUG_LOG_ERROR
            Debug.LogError("GameEntryLog - " + catetory.ToString() + args.Aggregate("", (current, message) => current + (" - " + message)));
#endif
        }

    }
}
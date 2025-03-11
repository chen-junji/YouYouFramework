using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


namespace YouYouFramework
{
    /// <summary>
    /// 游戏框架入口, 几乎所有框架模块都在这里调用
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        //全局参数设置
        [SerializeField]
        private ParamsSettings m_ParamsSettings;
        public static ParamsSettings ParamsSettings { get; private set; }

        [Header("UI摄像机")]
        public Camera UICamera;

        [Header("根画布")]
        public Canvas UIRootCanvas;

        [Header("根画布的缩放")]
        public CanvasScaler UIRootCanvasScaler;
        public RectTransform UIRootRectTransform { get; private set; }

        [Header("UI分组")]
        public UIGroup[] UIGroups;

        [Header("游戏物体对象池分组")]
        public SpawnPoolEntity[] GameObjectPoolGroups;

        [Header("声音主混合器")]
        public AudioMixer MasterMixer;


        //管理器属性
        public static TimeManager Time { get; private set; }
        public static FsmManager Fsm { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static DataTableManager DataTable { get; private set; }
        public static ModelManager Model { get; private set; }
        public static HttpManager Http { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static YouYouSceneManager Scene { get; private set; }
        public static LoaderManager Loader { get; private set; }
        public static UIManager UI { get; private set; }
        public static AudioManager Audio { get; private set; }
        public static PlayerPrefsManager PlayerPrefs { get; private set; }
        public static TaskManager Task { get; private set; }


        public static GameEntry Instance { get; private set; }
        private void Awake()
        {
            Log(LogCategory.Procedure, "GameEntry.OnAwake()");
            Instance = this;
            ParamsSettings = m_ParamsSettings;
            UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();

            //屏幕常亮
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        private void Start()
        {
            Log(LogCategory.Procedure, "GameEntry.OnStart()");

            //在new的构造函数中, 构造自身, 模块之间不可互相调用, 因为其他模块可能是null
            Time = new TimeManager();
            Fsm = new FsmManager();
            Procedure = new ProcedureManager();
            DataTable = new DataTableManager();
            Model = new ModelManager();
            Http = new HttpManager();
            Pool = new PoolManager();
            Scene = new YouYouSceneManager();
            Loader = new LoaderManager();
            UI = new UIManager();
            Audio = new AudioManager();
            PlayerPrefs = new PlayerPrefsManager();
            Task = new TaskManager();

            //在Init中, 模块之间可互相调用
            Procedure.Init();
            Audio.Init();

            //进入第一个流程
            Procedure.ChangeState(ProcedureManager.EState.Launch);
        }

        void Update()
        {
            Time.OnUpdate();
            Procedure.OnUpdate();
            Pool.OnUpdate();
            Scene.OnUpdate();
            UI.OnUpdate();
            Audio.OnUpdate();
            Task.OnUpdate();
        }
        private void OnApplicationQuit()
        {
            PlayerPrefs.SaveDataAll();
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                PlayerPrefs.SaveDataAll();
            }
        }

        public static void Log(object message)
        {
            Log(LogCategory.Normal, message);
        }
        public static void Log(LogCategory catetory, object message)
        {
#if DEBUG_LOG_NORMAL
            //由于性能原因，如果在Build Settings中没有勾上“Development Build”
            //即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            if (!Debug.isDebugBuild)
            {
                return;
            }

            Debug.Log($"GameEntryLog==>{catetory}==>{message}");
#endif
        }

        public static void LogWarning(LogCategory catetory, object message)
        {
#if DEBUG_LOG_WARNING
            Debug.LogWarning($"GameEntryLog==>{catetory}==>{message}");
#endif
        }

        public static void LogError(object message)
        {
            LogError(LogCategory.Normal, message);
        }
        public static void LogError(LogCategory catetory, object message)
        {
#if DEBUG_LOG_ERROR
            Debug.LogError($"GameEntryLog==>{catetory}==>{message}");
#endif
        }

    }
}
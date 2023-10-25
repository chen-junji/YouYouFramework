using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace YouYou
{
    public class GameEntry : MonoBehaviour
    {
        [FoldoutGroup("ResourceGroup")]
        [Header("游戏物体对象池分组")]
        public GameObjectPoolEntity[] GameObjectPoolGroups;

        [FoldoutGroup("ResourceGroup")]
        [Header("对象池锁定的资源包")]
        public string[] LockedAssetBundle;

        [FoldoutGroup("UIGroup")]
        [Header("UI摄像机")]
        public Camera UICamera;

        [FoldoutGroup("UIGroup")]
        [Header("根画布的缩放")]
        public CanvasScaler UIRootCanvasScaler;
        public RectTransform UIRootRectTransform { get; private set; }

        [FoldoutGroup("UIGroup")]
        [Header("UI分组")]
        public UIGroup[] UIGroups;

        [FoldoutGroup("AudioGroup")]
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
        public static HttpManager Http { get; private set; }
        public static DataManager Data { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static YouYouSceneManager Scene { get; private set; }
        public static LoaderManager Loader { get; private set; }
        public static UIManager UI { get; private set; }
        public static AudioManager Audio { get; private set; }
        public static CrossPlatformInputManager Input { get; private set; }
        public static TaskManager Task { get; private set; }
        public static QualityManager Quality { get; private set; }
        public static GuideManager Guide { get; private set; }
        public static ReddotMananger Reddot { get; private set; }


        /// <summary>
        /// 单例
        /// </summary>
        public static GameEntry Instance { get; private set; }

        private void Awake()
        {
            Log(LogCategory.Procedure, "GameEntry.OnAwake()");
            Instance = this;

            UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();

            CurrLanguage = m_CurrLanguage;
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
            Http = new HttpManager();
            Data = new DataManager();
            Localization = new LocalizationManager();
            Pool = new PoolManager();
            Scene = new YouYouSceneManager();
            Loader = new LoaderManager();
            UI = new UIManager();
            Audio = new AudioManager();
            Input = new CrossPlatformInputManager();
            Task = new TaskManager();
            Quality = new QualityManager();
            Guide = new GuideManager();
            Reddot = new ReddotMananger();

            Logger.Init();
            Procedure.Init();
            DataTable.Init();
            Http.Init();
            Data.Init();
            Localization.Init();
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
            Reddot.OnUpdate();
        }
        private void OnApplicationQuit()
        {
            Data.SaveDataAll();
            Logger.SyncLog();
            Logger.Dispose();
            Fsm.Dispose();
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause) Data.SaveDataAll();
        }


        public static void Log(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_NORMAL
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.Log(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        public static void LogWarning(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_WARNING
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogWarning(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        public static void LogError(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_ERROR
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogError(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

    }
}
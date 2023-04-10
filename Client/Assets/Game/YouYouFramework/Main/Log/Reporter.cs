#if !No_Reporter
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

//just drop this script to empty game object on first scene you game start at, this all what you have to do
//no coding is required 
//then you can view logs , warnings , errors and exceptions inside your game
//just draw circle on your game to view all logs
public class Reporter : MonoBehaviour
{
	public class Sample
	{
		public float time;
		public string loadedScene;
		public float memory;
		public float fps;
		public string fpsText;

		public static float MemSize()
		{
			float s = sizeof(float) + sizeof(byte) + sizeof(float) + sizeof(float);
			return s;
		}
	}

	List<Sample> samples = new List<Sample>(60 * 60 * 60);

	public class Log
	{
		public int count = 1;
		public LogType logType;
		public string condition;
		public string stacktrace;
		public int sampleId;
		//public string   objectName="" ;//object who send error
		//public string   rootName =""; //root of object send error

		public Log CreateCopy()
		{
			return (Log)this.MemberwiseClone();
		}

		public float GetMemoryUsage()
		{
			return (float)(sizeof(int) +
				sizeof(LogType) +
				condition.Length * sizeof(char) +
				stacktrace.Length * sizeof(char) +
				sizeof(int));
		}
	}
	//contains all uncollapsed log
	List<Log> logs = new List<Log>();
	//contains all collapsed logs
	List<Log> collapsedLogs = new List<Log>();
	//contain logs which should only appear to user , for example if you switch off show logs + switch off show warnings
	//and your mode is collapse,then this list will contains only collapsed errors
	List<Log> currentLog = new List<Log>();

	//used to check if the new coming logs is already exist or new one
	Dictionary<string, Log> logsDic = new Dictionary<string, Log>();
	//to save memory
	Dictionary<string, string> cachedString = new Dictionary<string, string>();
	[HideInInspector]
	//show hide In Game Logs
	public bool
		show = false;
	//collapse logs
	bool collapse;
	//to deside if you want to clean logs for new loaded scene
	bool clearOnNewSceneLoaded;
	bool showTime;
	bool showScene;
	bool showMemory;
	bool showFps;
	bool showGraph;

	//show or hide logs
	bool showLog = true;
	//show or hide warnings
	bool showWarning = true;
	//show or hide errors
	bool showError = true;

	//total number of logs
	int numOfLogs = 0;
	//total number of warnings
	int numOfLogsWarning = 0;
	//total number of errors
	int numOfLogsError = 0;
	//total number of collapsed logs
	int numOfCollapsedLogs = 0;
	//total number of collapsed warnings
	int numOfCollapsedLogsWarning = 0;
	//total number of collapsed errors
	int numOfCollapsedLogsError = 0;

	//maximum number of allowed logs to view
	//public int maxAllowedLog = 1000 ;


	string buildDate;
	string logDate;
	float logsMemUsage;
	float graphMemUsage;

	public float TotalMemUsage
	{
		get
		{
			return logsMemUsage + graphMemUsage;
		}
	}

	float gcTotalMemory;
	public string UserData = "";
	//fram rate per seconds
	public float fps;
	public string fpsText;

	//List<Texture2D> snapshots = new List<Texture2D>() ;

	enum ReportView
	{
		None,
		Logs,
		Info,
		Snapshot,
	}
	ReportView currentView = ReportView.Logs;
	enum DetailView
	{
		None,
		StackTrace,
		Graph,
	}

	//used to check if you have In Game Logs multiple time in different scene
	//only one should work and other should be deleted
	static bool created = false;
	//public delegate void OnLogHandler( string condition, string stacktrace, LogType type );
	//public event OnLogHandler OnLog ;

	// gui
	GUIContent clearContent;
	GUIContent collapseContent;
	GUIContent clearOnNewSceneContent;
	GUIContent showTimeContent;
	GUIContent showSceneContent;
	GUIContent userContent;
	GUIContent showMemoryContent;
	GUIContent softwareContent;
	GUIContent dateContent;
	GUIContent showFpsContent;
	GUIContent graphContent;
	GUIContent infoContent;
	GUIContent closeContent;
	GUIContent buildFromContent;
	GUIContent serverContent;
	GUIContent systemInfoContent;
	GUIContent graphicsInfoContent;
	GUIContent backContent;
	GUIContent cameraContent;
	Texture2D logIcon;
	Texture2D warningIcon;
	Texture2D errorIcon;
	GUIContent logContent;
	GUIContent warningContent;
	GUIContent errorContent;
	GUIContent cameraSwitch;


	GUIStyle barStyle;
	GUIStyle buttonActiveStyle;
	GUIStyle nonStyle;
	GUIStyle lowerLeftFontStyle;
	GUIStyle backStyle;
	GUIStyle evenLogStyle;
	GUIStyle oddLogStyle;
	GUIStyle logButtonStyle;
	GUIStyle selectedLogStyle;
	GUIStyle selectedLogFontStyle;
	GUIStyle stackLabelStyle;
	GUIStyle scrollerStyle;
	GUISkin toolbarScrollerSkin;
	GUISkin logScrollerSkin;
	GUISkin graphScrollerSkin;



	public Vector2 size = new Vector2(25, 32);
	public float maxSize = 20;
	public int numOfCircleToShow = 1;
	string currentScene;
	string deviceModel;
	string deviceType;
	string deviceName;
	string graphicsMemorySize;
	string maxTextureSize;
	string systemMemorySize;

	void Awake()
	{
		Initialize();
	}

	void OnEnable()
	{
		if (logs.Count == 0)//if recompile while in play mode
			clear();
	}

	void OnDisable()
	{

	}

	void addSample()
	{

		Sample sample = new Sample();
		sample.fps = fps;
		sample.fpsText = fpsText;
		sample.loadedScene = SceneManager.GetActiveScene().name;
		sample.time = Time.realtimeSinceStartup;
		sample.memory = gcTotalMemory;
		samples.Add(sample);

		graphMemUsage = (samples.Count * Sample.MemSize()) / 1024 / 1024;
	}

	public void Initialize()
	{
		if (!created)
		{
			currentScene = SceneManager.GetActiveScene().name;
			Application.logMessageReceived += CaptureLog;
			created = true;

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		int paddingX = (int)(size.x * 0.2f);
		int paddingY = (int)(size.y * 0.2f);
		//initialize gui and styles for gui porpose

		#region LoadImage
		m_TextureDic = new Dictionary<string, Texture2D>();

		string[] images = {
"clear",
"collapse",
"clearOnSceneLoaded",
"timer_1",
"UnityIcon",
"memory",
"software",
"date",
"fps",
"chart",
"info",
"close",
"user",
"buildFrom",
"ComputerIcon",
"graphicCard",
"back",
"ComputerIcon",
"log_icon",
"warning_icon",
"error_icon",
"bar",
"button_active",
"even_log",
"odd_log",
"selected",
"bar"
		};
		for (int i = 0; i < images.Length; i++)
		{
			string name = images[i];
			m_TextureDic[name] = Resources.Load<Texture2D>("Reporter/ReporterRes/" + name);
		}

		clearContent = new GUIContent("", getImage("clear", size.x, size.y), "Clear logs");
		collapseContent = new GUIContent("", getImage("collapse", size.x, size.y), "Collapse logs");
		clearOnNewSceneContent = new GUIContent("", getImage("clearOnSceneLoaded", size.x, size.y), "Clear logs on new scene loaded");
		showTimeContent = new GUIContent("", getImage("timer_1", size.x, size.y), "Show Hide Time");
		showSceneContent = new GUIContent("", getImage("UnityIcon", size.x, size.y), "Show Hide Scene");
		showMemoryContent = new GUIContent("", getImage("memory", size.x, size.y), "Show Hide Memory");
		softwareContent = new GUIContent("", getImage("software", size.x, size.y), "Software");
		dateContent = new GUIContent("", getImage("date", size.x, size.y), "Date");
		showFpsContent = new GUIContent("", getImage("fps", size.x, size.y), "Show Hide fps");
		graphContent = new GUIContent("", getImage("chart", size.x, size.y), "Show Graph");
		infoContent = new GUIContent("", getImage("info", size.x, size.y), "Information about application");
		closeContent = new GUIContent("", getImage("close", size.x, size.y), "Hide logs");
		userContent = new GUIContent("", getImage("user", size.x, size.y), "User");

		buildFromContent = new GUIContent("", getImage("buildFrom", size.x, size.y), "Build From");
		systemInfoContent = new GUIContent("", getImage("ComputerIcon", size.x, size.y), "System Info");
		graphicsInfoContent = new GUIContent("", getImage("graphicCard", size.x, size.y), "Graphics Info");
		backContent = new GUIContent("", getImage("back", size.x, size.y), "Back");

		serverContent = new GUIContent("", getImage("ComputerIcon", size.x, size.y), "重连到服务器");

		logIcon = getImage("log_icon", size.x, size.y);
		warningIcon = getImage("warning_icon", size.x, size.y);
		errorIcon = getImage("error_icon", size.x, size.y);

		//snapshotContent = new GUIContent("",getImage("camera" ,size.x,size.y),"show or hide logs");
		logContent = new GUIContent("", logIcon, "show or hide logs");
		warningContent = new GUIContent("", warningIcon, "show or hide warnings");
		errorContent = new GUIContent("", errorIcon, "show or hide errors");



		nonStyle = new GUIStyle();
		nonStyle.clipping = TextClipping.Clip;
		nonStyle.border = new RectOffset(0, 0, 0, 0);
		nonStyle.normal.background = null;
		nonStyle.fontSize = (int)(size.y / 2);
		nonStyle.alignment = TextAnchor.MiddleCenter;

		lowerLeftFontStyle = new GUIStyle();
		lowerLeftFontStyle.clipping = TextClipping.Clip;
		lowerLeftFontStyle.border = new RectOffset(0, 0, 0, 0);
		lowerLeftFontStyle.normal.background = null;
		lowerLeftFontStyle.fontSize = (int)(size.y / 2);
		lowerLeftFontStyle.fontStyle = FontStyle.Bold;
		lowerLeftFontStyle.alignment = TextAnchor.LowerLeft;


		barStyle = new GUIStyle();
		barStyle.border = new RectOffset(1, 1, 1, 1);
		barStyle.normal.background = getImage("bar", size.x, size.y);
		barStyle.active.background = getImage("button_active", size.x, size.y);
		barStyle.alignment = TextAnchor.MiddleCenter;
		barStyle.margin = new RectOffset(1, 1, 1, 1);

		//barStyle.padding = new RectOffset(paddingX,paddingX,paddingY,paddingY); 
		//barStyle.wordWrap = true ;
		barStyle.clipping = TextClipping.Clip;
		barStyle.fontSize = (int)(size.y / 2);


		buttonActiveStyle = new GUIStyle();
		buttonActiveStyle.border = new RectOffset(1, 1, 1, 1);
		buttonActiveStyle.normal.background = getImage("button_active", 32, 32);
		buttonActiveStyle.alignment = TextAnchor.MiddleCenter;
		buttonActiveStyle.margin = new RectOffset(1, 1, 1, 1);
		//buttonActiveStyle.padding = new RectOffset(4,4,4,4);
		buttonActiveStyle.fontSize = (int)(size.y / 2);

		backStyle = new GUIStyle();
		backStyle.normal.background = getImage("even_log", 16, 16);
		backStyle.clipping = TextClipping.Clip;
		backStyle.fontSize = (int)(size.y / 2);

		evenLogStyle = new GUIStyle();
		evenLogStyle.normal.background = getImage("even_log", 16, 16);
		evenLogStyle.fixedHeight = size.y;
		evenLogStyle.clipping = TextClipping.Clip;
		evenLogStyle.alignment = TextAnchor.UpperLeft;
		evenLogStyle.imagePosition = ImagePosition.ImageLeft;
		evenLogStyle.fontSize = (int)(size.y / 2);
		//evenLogStyle.wordWrap = true;

		oddLogStyle = new GUIStyle();
		oddLogStyle.normal.background = getImage("odd_log", 16, 16);
		oddLogStyle.fixedHeight = size.y;
		oddLogStyle.clipping = TextClipping.Clip;
		oddLogStyle.alignment = TextAnchor.UpperLeft;
		oddLogStyle.imagePosition = ImagePosition.ImageLeft;
		oddLogStyle.fontSize = (int)(size.y / 2);
		//oddLogStyle.wordWrap = true ;

		logButtonStyle = new GUIStyle();
		//logButtonStyle.wordWrap = true;
		logButtonStyle.fixedHeight = size.y;
		logButtonStyle.clipping = TextClipping.Clip;
		logButtonStyle.alignment = TextAnchor.UpperLeft;
		//logButtonStyle.imagePosition = ImagePosition.ImageLeft ;
		//logButtonStyle.wordWrap = true;
		logButtonStyle.fontSize = (int)(size.y / 2);
		logButtonStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		selectedLogStyle = new GUIStyle();
		selectedLogStyle.normal.background = getImage("selected", 16, 16);
		selectedLogStyle.fixedHeight = size.y;
		selectedLogStyle.clipping = TextClipping.Clip;
		selectedLogStyle.alignment = TextAnchor.UpperLeft;
		selectedLogStyle.normal.textColor = Color.white;
		//selectedLogStyle.wordWrap = true;
		selectedLogStyle.fontSize = (int)(size.y / 2);

		selectedLogFontStyle = new GUIStyle();
		selectedLogFontStyle.normal.background = getImage("selected", 16, 16);
		selectedLogFontStyle.fixedHeight = size.y;
		selectedLogFontStyle.clipping = TextClipping.Clip;
		selectedLogFontStyle.alignment = TextAnchor.UpperLeft;
		selectedLogFontStyle.normal.textColor = Color.white;
		//selectedLogStyle.wordWrap = true;
		selectedLogFontStyle.fontSize = (int)(size.y / 2);
		selectedLogFontStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		stackLabelStyle = new GUIStyle();
		stackLabelStyle.wordWrap = true;
		stackLabelStyle.fontSize = (int)(size.y / 2);
		stackLabelStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		scrollerStyle = new GUIStyle();
		scrollerStyle.normal.background = getImage("bar", size.x, size.y);
		#endregion

		#region LoadSkin
		GUISkin skin = Resources.Load<GUISkin>("Reporter/ReporterRes/reporterScrollerSkin");
		toolbarScrollerSkin = Instantiate(skin);
		toolbarScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		toolbarScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
		toolbarScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
		toolbarScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;

		logScrollerSkin = Instantiate(skin);
		logScrollerSkin.verticalScrollbar.fixedWidth = size.x * 2f;
		logScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
		logScrollerSkin.verticalScrollbarThumb.fixedWidth = size.x * 2f;
		logScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;

		graphScrollerSkin = Instantiate(skin);
		graphScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		graphScrollerSkin.horizontalScrollbar.fixedHeight = size.x * 2f;
		graphScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
		graphScrollerSkin.horizontalScrollbarThumb.fixedHeight = size.x * 2f;

		currentView = (ReportView)PlayerPrefs.GetInt("Reporter_currentView", 1);
		show = (PlayerPrefs.GetInt("Reporter_show") == 1) ? true : false;
		collapse = (PlayerPrefs.GetInt("Reporter_collapse") == 1) ? true : false;
		clearOnNewSceneLoaded = (PlayerPrefs.GetInt("Reporter_clearOnNewSceneLoaded") == 1) ? true : false;
		showTime = (PlayerPrefs.GetInt("Reporter_showTime") == 1) ? true : false;
		showScene = (PlayerPrefs.GetInt("Reporter_showScene") == 1) ? true : false;
		showMemory = (PlayerPrefs.GetInt("Reporter_showMemory") == 1) ? true : false;
		showFps = (PlayerPrefs.GetInt("Reporter_showFps") == 1) ? true : false;
		showGraph = (PlayerPrefs.GetInt("Reporter_showGraph") == 1) ? true : false;
		showLog = (PlayerPrefs.GetInt("Reporter_showLog", 1) == 1) ? true : false;
		showWarning = (PlayerPrefs.GetInt("Reporter_showWarning", 1) == 1) ? true : false;
		showError = (PlayerPrefs.GetInt("Reporter_showError", 1) == 1) ? true : false;

		if (show)
		{
			doShow();
		}

		deviceModel = SystemInfo.deviceModel.ToString();
		deviceType = SystemInfo.deviceType.ToString();
		deviceName = SystemInfo.deviceName.ToString();
		graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
		maxTextureSize = SystemInfo.maxTextureSize.ToString();
		systemMemorySize = SystemInfo.systemMemorySize.ToString();
		#endregion
	}

	//some library or plugin in unity3d take the capture of the log
	//logViewer take the capture every second , but may be you loose the log for 1 second
	//you can use this method to force the log to be yours again
	public void CatureLog()
	{
		Application.logMessageReceived += CaptureLog;
	}

	void Start()
	{
		logDate = System.DateTime.Now.ToString();
		StartCoroutine("readInfo");
		//ActiveCameraOperator();
	}

	//clear all logs
	void clear()
	{
		logs.Clear();
		collapsedLogs.Clear();
		currentLog.Clear();
		logsDic.Clear();
		selectedIndex = -1;
		numOfLogs = 0;
		numOfLogsWarning = 0;
		numOfLogsError = 0;
		numOfCollapsedLogs = 0;
		numOfCollapsedLogsWarning = 0;
		numOfCollapsedLogsError = 0;
		logsMemUsage = 0;
		graphMemUsage = 0;
		samples.Clear();
		System.GC.Collect();
		selectedLog = null;
	}

	Rect screenRect;
	Rect toolBarRect;
	Rect logsRect;
	Rect stackRect;
	Rect graphRect;
	Rect graphMinRect;
	Rect graphMaxRect;
	Rect buttomRect;
	Vector2 stackRectTopLeft;
	Rect detailRect;
	Vector2 scrollPosition;
	Vector2 scrollPosition2;
	Vector2 toolbarScrollPosition;
	int selectedIndex = -1;
	Log selectedLog;
	float toolbarOldDrag = 0;
	float oldDrag;
	float oldDrag2;
	float oldDrag3;
	int startIndex;

	private Dictionary<string, Texture2D> m_TextureDic;
	//try to make texture GUI type 
	Texture2D getImage(string path, float width, float height)
	{
		return m_TextureDic[path];
	}


	//calculate what is the currentLog : collapsed or not , hide or view warnings ......
	void calculateCurrentLog()
	{
		currentLog.Clear();
		if (collapse)
		{
			for (int i = 0; i < collapsedLogs.Count; i++)
			{
				Log log = collapsedLogs[i];
				if (log.logType == LogType.Log && !showLog)
					continue;
				if (log.logType == LogType.Warning && !showWarning)
					continue;
				if (log.logType == LogType.Error && !showError)
					continue;
				if (log.logType == LogType.Assert && !showError)
					continue;
				if (log.logType == LogType.Exception && !showError)
					continue;


				currentLog.Add(log);
			}
		}
		else
		{
			for (int i = 0; i < logs.Count; i++)
			{
				Log log = logs[i];
				if (log.logType == LogType.Log && !showLog)
					continue;
				if (log.logType == LogType.Warning && !showWarning)
					continue;
				if (log.logType == LogType.Error && !showError)
					continue;
				if (log.logType == LogType.Assert && !showError)
					continue;
				if (log.logType == LogType.Exception && !showError)
					continue;

				currentLog.Add(log);
			}
		}
	}

	Rect countRect;
	Rect timeRect;
	Rect timeLabelRect;
	Rect sceneRect;
	Rect sceneLabelRect;
	Rect memoryRect;
	Rect memoryLabelRect;
	Rect fpsRect;
	Rect fpsLabelRect;
	GUIContent tempContent = new GUIContent();
	Vector2 infoScrollPosition;
	Vector2 oldInfoDrag;

	string DownloadServer = "";
	void DrawInfo()
	{

		GUILayout.BeginArea(screenRect, backStyle);

		Vector2 drag = getDrag();
		if ((drag.x != 0) && (downPos != Vector2.zero))
		{
			infoScrollPosition.x -= (drag.x - oldInfoDrag.x);
		}
		if ((drag.y != 0) && (downPos != Vector2.zero))
		{
			infoScrollPosition.y += (drag.y - oldInfoDrag.y);
		}
		oldInfoDrag = drag;

		GUI.skin = toolbarScrollerSkin;
		infoScrollPosition = GUILayout.BeginScrollView(infoScrollPosition);
		GUILayout.Space(size.x);
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(buildFromContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(buildDate, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(systemInfoContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceModel, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceType, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(deviceName, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(graphicsInfoContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(SystemInfo.graphicsDeviceName, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(graphicsMemorySize, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(maxTextureSize, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Label("Screen Width " + Screen.width, nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("Screen Height " + Screen.height, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(systemMemorySize + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Space(size.x);
		GUILayout.Label("Mem Usage Of Logs " + logsMemUsage.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("Mem Usage Of Graph " + graphMemUsage.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label("GC Memory " + gcTotalMemory.ToString("0.000") + " mb", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(softwareContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(SystemInfo.operatingSystem, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(dateContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(System.DateTime.Now.ToString(), nonStyle, GUILayout.Height(size.y));
		GUILayout.Label(" - Application Started At " + logDate, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(Time.realtimeSinceStartup.ToString("000"), nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(fpsText, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(userContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(UserData, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Space(size.x);
		GUILayout.Label(currentScene, nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Open Camera", GUILayout.Height(60)))
		{
			//Handheld.PlayFullScreenMovie("Mov/CG.mp4");          
			//ActiveCameraOperator();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		/*GUILayout.BeginHorizontal();
        GUILayout.Space( size.x);
        GUILayout.Box( graphContent ,nonStyle ,  GUILayout.Width(size.x) , GUILayout.Height(size.y));
        GUILayout.Space( size.x);
        GUILayout.Label( "frame " + samples.Count , nonStyle , GUILayout.Height(size.y));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();*/

		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.Space(size.x);
		if (GUILayout.Button(backContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			currentView = ReportView.Logs;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();

		GUILayout.EndArea();
	}

	bool cameraShow = true;

	void DrawReport()
	{
		screenRect.x = 0f;
		screenRect.y = 0f;
		screenRect.width = Screen.width;
		screenRect.height = Screen.height;
		GUILayout.BeginArea(screenRect, backStyle);
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box(cameraContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.Label("Select Photo", nonStyle, GUILayout.Height(size.y));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Comming Soon", nonStyle, GUILayout.Height(size.y));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(backContent, barStyle, GUILayout.Width(size.x), GUILayout.Height(size.y)))
		{
			currentView = ReportView.Logs;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	void drawToolBar()
	{

		toolBarRect.x = 0f;
		toolBarRect.y = 0f;
		toolBarRect.width = Screen.width;
		toolBarRect.height = size.y * 2f;

		//toolbarScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		//toolbarScrollerSkin.horizontalScrollbar.fixedHeight= 0f  ;

		GUI.skin = toolbarScrollerSkin;
		Vector2 drag = getDrag();
		if ((drag.x != 0) && (downPos != Vector2.zero) && (downPos.y > Screen.height - size.y * 2f))
		{
			toolbarScrollPosition.x -= (drag.x - toolbarOldDrag);
		}
		toolbarOldDrag = drag.x;
		GUILayout.BeginArea(toolBarRect);
		toolbarScrollPosition = GUILayout.BeginScrollView(toolbarScrollPosition);
		GUILayout.BeginHorizontal(barStyle);

		if (GUILayout.Button(clearContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			clear();
		}
		if (GUILayout.Button(collapseContent, (collapse) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			collapse = !collapse;
			calculateCurrentLog();
		}
		if (GUILayout.Button(clearOnNewSceneContent, (clearOnNewSceneLoaded) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			clearOnNewSceneLoaded = !clearOnNewSceneLoaded;
		}

		if (GUILayout.Button(showTimeContent, (showTime) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showTime = !showTime;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, Time.realtimeSinceStartup.ToString("0.0"), lowerLeftFontStyle);
		if (GUILayout.Button(showSceneContent, (showScene) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showScene = !showScene;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, currentScene, lowerLeftFontStyle);
		if (GUILayout.Button(showMemoryContent, (showMemory) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showMemory = !showMemory;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, gcTotalMemory.ToString("0.0"), lowerLeftFontStyle);

		if (GUILayout.Button(showFpsContent, (showFps) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showFps = !showFps;
		}
		tempRect = GUILayoutUtility.GetLastRect();
		GUI.Label(tempRect, fpsText, lowerLeftFontStyle);
		/*if( GUILayout.Button( graphContent , (showGraph)?buttonActiveStyle:barStyle , GUILayout.Width(size.x*2) ,GUILayout.Height(size.y*2)))
        {
            showGraph = !showGraph ;
        }
        tempRect = GUILayoutUtility.GetLastRect();
        GUI.Label( tempRect , samples.Count.ToString() , lowerLeftFontStyle );*/
		if (GUILayout.Button(infoContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			currentView = ReportView.Info;
		}

		GUILayout.FlexibleSpace();


		string logsText = " ";
		if (collapse)
		{
			logsText += numOfCollapsedLogs;
		}
		else
		{
			logsText += numOfLogs;
		}
		string logsWarningText = " ";
		if (collapse)
		{
			logsWarningText += numOfCollapsedLogsWarning;
		}
		else
		{
			logsWarningText += numOfLogsWarning;
		}
		string logsErrorText = " ";
		if (collapse)
		{
			logsErrorText += numOfCollapsedLogsError;
		}
		else
		{
			logsErrorText += numOfLogsError;
		}

		GUILayout.BeginHorizontal((showLog) ? buttonActiveStyle : barStyle);
		if (GUILayout.Button(logContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showLog = !showLog;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showLog = !showLog;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((showWarning) ? buttonActiveStyle : barStyle);
		if (GUILayout.Button(warningContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsWarningText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((showError) ? buttonActiveStyle : nonStyle);
		if (GUILayout.Button(errorContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showError = !showError;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsErrorText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			showError = !showError;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();

		if (GUILayout.Button(closeContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			Close();
		}


		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();

		GUILayout.EndArea();
	}

	public void Close()
	{
		show = false;
		ReporterGUI gui = gameObject.GetComponent<ReporterGUI>();
		DestroyImmediate(gui);
	}

	Rect tempRect;

	void DrawLogs()
	{

		GUILayout.BeginArea(logsRect, backStyle);

		GUI.skin = logScrollerSkin;
		//setStartPos();
		Vector2 drag = getDrag();

		if (drag.y != 0 && logsRect.Contains(new Vector2(downPos.x, Screen.height - downPos.y)))
		{
			scrollPosition.y += (drag.y - oldDrag);
		}
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		oldDrag = drag.y;


		int totalVisibleCount = (int)(Screen.height * 0.75f / size.y);
		int totalCount = getTotalCount();
		/*if( totalCount < 100 )
            inGameLogsScrollerSkin.verticalScrollbarThumb.fixedHeight = 0;
        else 
            inGameLogsScrollerSkin.verticalScrollbarThumb.fixedHeight = 64;*/

		totalVisibleCount = Mathf.Min(totalVisibleCount, totalCount - startIndex);
		int index = 0;
		int beforeHeight = (int)(startIndex * size.y);
		selectedIndex = Mathf.Clamp(selectedIndex, -1, totalCount - 1);
		if (beforeHeight > 0)
		{
			//fill invisible gap befor scroller to make proper scroller pos
			GUILayout.BeginHorizontal(GUILayout.Height(beforeHeight));
			GUILayout.Label("---");
			GUILayout.EndHorizontal();
		}

		int endIndex = startIndex + totalVisibleCount;
		endIndex = Mathf.Clamp(endIndex, 0, totalCount);
		bool scrollerVisible = (totalVisibleCount < totalCount);
		for (int i = startIndex; (startIndex + index) < endIndex; i++)
		{

			if (i >= currentLog.Count)
				break;
			Log log = currentLog[i];

			if (log.logType == LogType.Log && !showLog)
				continue;
			if (log.logType == LogType.Warning && !showWarning)
				continue;
			if (log.logType == LogType.Error && !showError)
				continue;
			if (log.logType == LogType.Assert && !showError)
				continue;
			if (log.logType == LogType.Exception && !showError)
				continue;

			if (index >= totalVisibleCount)
			{
				break;
			}

			GUIContent content = null;
			if (log.logType == LogType.Log)
				content = logContent;
			else if (log.logType == LogType.Warning)
				content = warningContent;
			else
				content = errorContent;
			//content.text = log.condition ;

			GUIStyle currentLogStyle = ((startIndex + index) % 2 == 0) ? evenLogStyle : oddLogStyle;
			if (selectedIndex == (startIndex + index))
			{
				selectedLog = log;
				currentLogStyle = selectedLogStyle;
			}
			else
			{
			}

			tempContent.text = log.count.ToString();
			float w = 0f;
			if (collapse)
				w = barStyle.CalcSize(tempContent).x + 3;
			countRect.x = Screen.width - w;
			countRect.y = size.y * i;
			if (beforeHeight > 0)
				countRect.y += 8;//i will check later why
			countRect.width = w;
			countRect.height = size.y;

			if (scrollerVisible)
				countRect.x -= size.x * 2;

			Sample sample = samples[log.sampleId];
			fpsRect = countRect;
			if (showFps)
			{
				tempContent.text = sample.fpsText;
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				fpsRect.x -= w;
				fpsRect.width = size.x;
				fpsLabelRect = fpsRect;
				fpsLabelRect.x += size.x;
				fpsLabelRect.width = w - size.x;
			}


			memoryRect = fpsRect;
			if (showMemory)
			{
				tempContent.text = sample.memory.ToString("0.000");
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				memoryRect.x -= w;
				memoryRect.width = size.x;
				memoryLabelRect = memoryRect;
				memoryLabelRect.x += size.x;
				memoryLabelRect.width = w - size.x;
			}
			sceneRect = memoryRect;
			if (showScene)
			{
				tempContent.text = sample.loadedScene;
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				sceneRect.x -= w;
				sceneRect.width = size.x;
				sceneLabelRect = sceneRect;
				sceneLabelRect.x += size.x;
				sceneLabelRect.width = w - size.x;
			}
			timeRect = sceneRect;
			if (showTime)
			{
				tempContent.text = sample.time.ToString("0.000");
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				timeRect.x -= w;
				timeRect.width = size.x;
				timeLabelRect = timeRect;
				timeLabelRect.x += size.x;
				timeLabelRect.width = w - size.x;
			}



			GUILayout.BeginHorizontal(currentLogStyle);
			if (selectedIndex == (startIndex + index))
			{
				GUILayout.Box(content, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
				GUILayout.Label(log.condition, selectedLogFontStyle);
				//GUILayout.FlexibleSpace();
				if (showTime)
				{
					GUI.Box(timeRect, showTimeContent, currentLogStyle);
					GUI.Label(timeLabelRect, sample.time.ToString("0.000"), currentLogStyle);
				}
				if (showScene)
				{
					GUI.Box(sceneRect, showSceneContent, currentLogStyle);
					GUI.Label(sceneLabelRect, sample.loadedScene, currentLogStyle);
				}
				if (showMemory)
				{
					GUI.Box(memoryRect, showMemoryContent, currentLogStyle);
					GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", currentLogStyle);
				}
				if (showFps)
				{
					GUI.Box(fpsRect, showFpsContent, currentLogStyle);
					GUI.Label(fpsLabelRect, sample.fpsText, currentLogStyle);
				}


			}
			else
			{
				if (GUILayout.Button(content, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y)))
					selectedIndex = startIndex + index;
				if (GUILayout.Button(log.condition, logButtonStyle))
					selectedIndex = startIndex + index;
				//GUILayout.FlexibleSpace();
				if (showTime)
				{
					GUI.Box(timeRect, showTimeContent, currentLogStyle);
					GUI.Label(timeLabelRect, sample.time.ToString("0.000"), currentLogStyle);
				}
				if (showScene)
				{
					GUI.Box(sceneRect, showSceneContent, currentLogStyle);
					GUI.Label(sceneLabelRect, sample.loadedScene, currentLogStyle);
				}
				if (showMemory)
				{
					GUI.Box(memoryRect, showMemoryContent, currentLogStyle);
					GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", currentLogStyle);
				}
				if (showFps)
				{
					GUI.Box(fpsRect, showFpsContent, currentLogStyle);
					GUI.Label(fpsLabelRect, sample.fpsText, currentLogStyle);
				}
			}
			if (collapse)
				GUI.Label(countRect, log.count.ToString(), barStyle);
			GUILayout.EndHorizontal();
			index++;
		}

		int afterHeight = (int)((totalCount - (startIndex + totalVisibleCount)) * size.y);
		if (afterHeight > 0)
		{
			//fill invisible gap after scroller to make proper scroller pos
			GUILayout.BeginHorizontal(GUILayout.Height(afterHeight));
			GUILayout.Label(" ");
			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();

		buttomRect.x = 0f;
		buttomRect.y = Screen.height - size.y;
		buttomRect.width = Screen.width;
		buttomRect.height = size.y;

		if (showGraph)
			drawGraph();
		else
			drawStack();
	}

	float graphSize = 4f;
	int startFrame = 0;
	int currentFrame = 0;
	Vector3 tempVector1;
	Vector3 tempVector2;
	Vector2 graphScrollerPos;
	float maxFpsValue;
	float minFpsValue;
	float maxMemoryValue;
	float minMemoryValue;

	void drawGraph()
	{

		graphRect = stackRect;
		graphRect.height = Screen.height * 0.25f;//- size.y ;



		//startFrame = samples.Count - (int)(Screen.width / graphSize) ;
		//if( startFrame < 0 ) startFrame = 0 ;
		GUI.skin = graphScrollerSkin;

		Vector2 drag = getDrag();
		if (graphRect.Contains(new Vector2(downPos.x, Screen.height - downPos.y)))
		{
			if (drag.x != 0)
			{
				graphScrollerPos.x -= drag.x - oldDrag3;
				graphScrollerPos.x = Mathf.Max(0, graphScrollerPos.x);
			}

			Vector2 p = downPos;
			if (p != Vector2.zero)
			{
				currentFrame = startFrame + (int)(p.x / graphSize);
			}
		}

		oldDrag3 = drag.x;
		GUILayout.BeginArea(graphRect, backStyle);

		graphScrollerPos = GUILayout.BeginScrollView(graphScrollerPos);
		startFrame = (int)(graphScrollerPos.x / graphSize);
		if (graphScrollerPos.x >= (samples.Count * graphSize - Screen.width))
			graphScrollerPos.x += graphSize;

		GUILayout.Label(" ", GUILayout.Width(samples.Count * graphSize));
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		maxFpsValue = 0;
		minFpsValue = 100000;
		maxMemoryValue = 0;
		minMemoryValue = 100000;
		for (int i = 0; i < Screen.width / graphSize; i++)
		{
			int index = startFrame + i;
			if (index >= samples.Count)
				break;
			Sample s = samples[index];
			if (maxFpsValue < s.fps)
				maxFpsValue = s.fps;
			if (minFpsValue > s.fps)
				minFpsValue = s.fps;
			if (maxMemoryValue < s.memory)
				maxMemoryValue = s.memory;
			if (minMemoryValue > s.memory)
				minMemoryValue = s.memory;
		}

		//GUI.BeginGroup(graphRect);


		if (currentFrame != -1 && currentFrame < samples.Count)
		{
			Sample selectedSample = samples[currentFrame];
			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.BeginHorizontal();

			GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.time.ToString("0.0"), nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.loadedScene, nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.memory.ToString("0.000"), nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.fpsText, nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(graphContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(currentFrame.ToString(), nonStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		graphMaxRect = stackRect;
		graphMaxRect.height = size.y;
		GUILayout.BeginArea(graphMaxRect);
		GUILayout.BeginHorizontal();

		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(maxMemoryValue.ToString("0.000"), nonStyle);

		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
		GUILayout.Label(maxFpsValue.ToString("0.000"), nonStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		graphMinRect = stackRect;
		graphMinRect.y = stackRect.y + stackRect.height - size.y;
		graphMinRect.height = size.y;
		GUILayout.BeginArea(graphMinRect);
		GUILayout.BeginHorizontal();

		GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

		GUILayout.Label(minMemoryValue.ToString("0.000"), nonStyle);


		GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));

		GUILayout.Label(minFpsValue.ToString("0.000"), nonStyle);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		//GUI.EndGroup();
	}

	void drawStack()
	{

		if (selectedIndex != -1 && selectedLog != null)
		{
			Vector2 drag = getDrag();
			if (drag.y != 0 && stackRect.Contains(new Vector2(downPos.x, Screen.height - downPos.y)))
			{
				scrollPosition2.y += drag.y - oldDrag2;
			}
			oldDrag2 = drag.y;



			GUILayout.BeginArea(stackRect, backStyle);
			scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
			Sample selectedSample = null;
			try
			{
				selectedSample = samples[selectedLog.sampleId];
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label(selectedLog.condition, stackLabelStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y * 0.25f);
			GUILayout.BeginHorizontal();
			GUILayout.Label(selectedLog.stacktrace, stackLabelStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y);
			GUILayout.EndScrollView();
			GUILayout.EndArea();


			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.BeginHorizontal();

			GUILayout.Box(showTimeContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.time.ToString("0.0"), nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showSceneContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.loadedScene, nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showMemoryContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.memory.ToString("0.000"), nonStyle);
			GUILayout.Space(size.x);

			GUILayout.Box(showFpsContent, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
			GUILayout.Label(selectedSample.fpsText, nonStyle);
			/*GUILayout.Space( size.x );
            GUILayout.Box( graphContent ,nonStyle, GUILayout.Width(size.x) ,GUILayout.Height(size.y));
            GUILayout.Label( selectedLog.sampleId.ToString() ,nonStyle  );*/
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();



		}
		else
		{
			GUILayout.BeginArea(stackRect, backStyle);
			GUILayout.EndArea();
			GUILayout.BeginArea(buttomRect, backStyle);
			GUILayout.EndArea();
		}

	}

	public void OnGUIDraw()
	{

		if (!show)
		{
			return;
		}

		screenRect.x = 0;
		screenRect.y = 0;
		screenRect.width = Screen.width;
		screenRect.height = Screen.height;

		getDownPos();


		logsRect.x = 0f;
		logsRect.y = size.y * 2f;
		logsRect.width = Screen.width;
		logsRect.height = Screen.height * 0.75f - size.y * 2f;

		stackRectTopLeft.x = 0f;
		stackRect.x = 0f;
		stackRectTopLeft.y = Screen.height * 0.75f;
		stackRect.y = Screen.height * 0.75f;
		stackRect.width = Screen.width;
		stackRect.height = Screen.height * 0.25f - size.y;



		detailRect.x = 0f;
		detailRect.y = Screen.height - size.y * 3;
		detailRect.width = Screen.width;
		detailRect.height = size.y * 3;

		if (currentView == ReportView.Info)
			DrawInfo();
		else if (currentView == ReportView.Logs)
		{
			drawToolBar();
			DrawLogs();
		}


	}

	List<Vector2> gestureDetector = new List<Vector2>();
	Vector2 gestureSum = Vector2.zero;
	float gestureLength = 0;
	int gestureCount = 0;

	//    bool isGestureDone()
	//    {
	//        if (Application.platform == RuntimePlatform.Android || 
	//            Application.platform == RuntimePlatform.IPhonePlayer)
	//        {
	//            if (Input.touches.Length != 1)
	//            {
	//                gestureDetector.Clear();
	//                gestureCount = 0;
	//            } else
	//            {
	//                if (Input.touches [0].phase == TouchPhase.Canceled || Input.touches [0].phase == TouchPhase.Ended)
	//                    gestureDetector.Clear();
	//                else if (Input.touches [0].phase == TouchPhase.Moved)
	//                {
	//                    Vector2 p = Input.touches [0].position;
	//                    if (gestureDetector.Count == 0 || (p - gestureDetector [gestureDetector.Count - 1]).magnitude > 10)
	//                        gestureDetector.Add(p);
	//                }
	//            }
	//        } else
	//        {
	//            if (Input.GetMouseButtonUp(0))
	//            {
	//                gestureDetector.Clear();
	//                gestureCount = 0;
	//            } else
	//            {
	//                if (Input.GetMouseButton(0))
	//                {
	//                    Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	//                    if (gestureDetector.Count == 0 || (p - gestureDetector [gestureDetector.Count - 1]).magnitude > 10)
	//                        gestureDetector.Add(p);
	//                }
	//            }
	//        }
	//        
	//        if (gestureDetector.Count < 10)
	//            return false;
	//        
	//        gestureSum = Vector2.zero;
	//        gestureLength = 0;
	//        Vector2 prevDelta = Vector2.zero;
	//        for (int i = 0; i < gestureDetector.Count - 2; i++)
	//        {
	//            
	//            Vector2 delta = gestureDetector [i + 1] - gestureDetector [i];
	//            float deltaLength = delta.magnitude;
	//            gestureSum += delta; 
	//            gestureLength += deltaLength; 
	//            
	//            float dot = Vector2.Dot(delta, prevDelta);
	//            if (dot < 0f)
	//            {
	//                gestureDetector.Clear();
	//                gestureCount = 0;
	//                return false;
	//            }
	//            
	//            prevDelta = delta;
	//        }
	//        
	//        int gestureBase = (Screen.width + Screen.height) / 4;
	//        
	//        if (gestureLength > gestureBase && gestureSum.magnitude < gestureBase / 2)
	//        {
	//            gestureDetector.Clear();
	//            gestureCount++;
	//            if (gestureCount >= numOfCircleToShow) 
	//                return true;
	//        }
	//        
	//        return false;
	//    }
	bool isGestureDone()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				gestureDetector.Clear();
				gestureCount = 0;
			}
			else
			{
				if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[0].phase == TouchPhase.Ended)
					gestureDetector.Clear();
				else if (Input.touches[0].phase == TouchPhase.Moved)
				{
					Vector2 p = Input.touches[0].position;
					if (gestureDetector.Count == 0 || (p - gestureDetector[gestureDetector.Count - 1]).magnitude > (Screen.width * 0.8f))
					{
						gestureDetector.Add(p);
					}
				}
			}
		}
		else
		{
			if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			   && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			   && Input.GetKey(KeyCode.D))
			{
				return true;
			}
			if (Input.GetMouseButtonUp(0))
			{
				gestureDetector.Clear();
				gestureCount = 0;
			}
			else
			{
				if (Input.GetMouseButton(0))
				{
					Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
					if (gestureDetector.Count == 0 || (p - gestureDetector[gestureDetector.Count - 1]).magnitude > (Screen.width * 0.8f))
					{
						gestureDetector.Add(p);
					}
				}
			}
		}

		if (gestureDetector.Count != 2)
		{
			return false;
		}

		gestureLength = (gestureDetector[1] - gestureDetector[0]).magnitude;
		if (gestureLength > (Screen.width * 0.8f))
		{
			gestureDetector.Clear();
			gestureCount = 0;
			return true;
		}

		gestureDetector.Clear();
		gestureCount = 0;
		return false;
	}

	float lastClickTime = -1;

	bool isDoubleClickDone()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				lastClickTime = -1;
			}
			else
			{
				if (Input.touches[0].phase == TouchPhase.Began)
				{
					if (lastClickTime == -1)
						lastClickTime = Time.realtimeSinceStartup;
					else if (Time.realtimeSinceStartup - lastClickTime < 0.2f)
					{
						lastClickTime = -1;
						return true;
					}
					else
					{
						lastClickTime = Time.realtimeSinceStartup;
					}
				}
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (lastClickTime == -1)
					lastClickTime = Time.realtimeSinceStartup;
				else if (Time.realtimeSinceStartup - lastClickTime < 0.2f)
				{
					lastClickTime = -1;
					return true;
				}
				else
				{
					lastClickTime = Time.realtimeSinceStartup;
				}
			}
		}
		return false;
	}

	int getTotalCount()
	{
		int count = 0;
		if (collapse)
		{
			if (showLog)
				count += numOfCollapsedLogs;
			if (showWarning)
				count += numOfCollapsedLogsWarning;
			if (showError)
				count += numOfCollapsedLogsError;
		}
		else
		{
			if (showLog)
				count += numOfLogs;
			if (showWarning)
				count += numOfLogsWarning;
			if (showError)
				count += numOfLogsError;
		}
		return count;
	}

	//calculate  pos of first click on screen
	Vector2 startPos;
	Vector2 downPos;

	Vector2 getDownPos()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{

			if (Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began)
			{
				downPos = Input.touches[0].position;
				return downPos;
			}
		}
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				downPos.x = Input.mousePosition.x;
				downPos.y = Input.mousePosition.y;
				return downPos;
			}
		}

		return Vector2.zero;
	}
	//calculate drag amount , this is used for scrolling

	Vector2 mousePosition;

	Vector2 getDrag()
	{

		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touches.Length != 1)
			{
				return Vector2.zero;
			}
			return Input.touches[0].position - downPos;
		}
		else
		{
			if (Input.GetMouseButton(0))
			{
				mousePosition = Input.mousePosition;
				return mousePosition - downPos;
			}
			else
			{
				return Vector2.zero;
			}
		}
	}

	//calculate the start index of visible log
	void calculateStartIndex()
	{
		startIndex = (int)(scrollPosition.y / size.y);
		startIndex = Mathf.Clamp(startIndex, 0, getTotalCount());
	}

	float lastUpdate = 0;
	float lastUpdate2 = 0;
	Rect temp;

	void doShow()
	{
		show = true;
		currentView = ReportView.Logs;
		gameObject.AddComponent<ReporterGUI>();
	}

	void Update()
	{
		fpsText = fps.ToString("0.000");
		gcTotalMemory = (((float)System.GC.GetTotalMemory(false)) / 1024 / 1024);
		//addSample();

		float elapsed = Time.realtimeSinceStartup - lastUpdate;
		fps = 1f / elapsed;
		lastUpdate = Time.realtimeSinceStartup;
		calculateStartIndex();
		if (!show && isGestureDone())
		{
#if !UNITY_EDITOR && DEBUG_MODEL
			doShow();
#endif
		}


		float elapsed2 = Time.realtimeSinceStartup - lastUpdate2;
		if (elapsed2 > 1)
		{
			lastUpdate2 = Time.realtimeSinceStartup;
			//be sure no body else take control of log 
			//            Application.RegisterLogCallback(new Application.LogCallback(CaptureLog));
		}
	}

	public void CaptureLog(string condition, string stacktrace, LogType type)
	{
		Regex reg = new Regex("\\<color.*?\\>", RegexOptions.IgnoreCase);

		condition = reg.Replace(condition, "");
		condition = condition.Replace("</color>", "");

		float memUsage = 0f;
		string _condition = "";
		if (cachedString.ContainsKey(condition))
		{
			_condition = cachedString[condition];
		}
		else
		{
			_condition = condition;
			cachedString.Add(_condition, _condition);
			memUsage += (string.IsNullOrEmpty(_condition) ? 0 : _condition.Length * sizeof(char));
			memUsage += System.IntPtr.Size;
		}
		string _stacktrace = "";
		if (cachedString.ContainsKey(stacktrace))
		{
			_stacktrace = cachedString[stacktrace];
		}
		else
		{
			_stacktrace = stacktrace;
			cachedString.Add(_stacktrace, _stacktrace);
			memUsage += (string.IsNullOrEmpty(_stacktrace) ? 0 : _stacktrace.Length * sizeof(char));
			memUsage += System.IntPtr.Size;
		}
		bool newLogAdded = false;

		addSample();
		Log log = new Log() { logType = (LogType)type, condition = _condition, stacktrace = _stacktrace, sampleId = samples.Count - 1 };
		memUsage += log.GetMemoryUsage();
		//memUsage += samples.Count * 13 ;

		logsMemUsage += memUsage / 1024 / 1024;

		if (TotalMemUsage > maxSize)
		{
			clear();
			//Debuger.Log("Memory Usage Reach" + maxSize + " mb So It is Cleared");
			return;
		}

		bool isNew = false;
		//string key = _condition;// + "_!_" + _stacktrace ;
		if (logsDic.ContainsKey(_condition) && logsDic[_condition].stacktrace == stacktrace)
		{
			isNew = false;
			logsDic[_condition].count++;
		}
		else
		{
			isNew = true;
			collapsedLogs.Add(log);
			logsDic[_condition] = log;

			if (type == LogType.Log)
				numOfCollapsedLogs++;
			else if (type == LogType.Warning)
				numOfCollapsedLogsWarning++;
			else
				numOfCollapsedLogsError++;
		}

		if (type == LogType.Log)
			numOfLogs++;
		else if (type == LogType.Warning)
			numOfLogsWarning++;
		else
			numOfLogsError++;


		logs.Add(log);
		if (!collapse || isNew)
		{
			bool skip = false;
			if (log.logType == LogType.Log && !showLog)
				skip = true;
			if (log.logType == LogType.Warning && !showWarning)
				skip = true;
			if (log.logType == LogType.Error && !showError)
				skip = true;
			if (log.logType == LogType.Assert && !showError)
				skip = true;
			if (log.logType == LogType.Exception && !showError)
				skip = true;

			if (!skip)
			{
				currentLog.Add(log);
				newLogAdded = true;
			}
		}

		if (newLogAdded)
		{
			calculateStartIndex();
			int totalCount = getTotalCount();
			int totalVisibleCount = (int)(Screen.height * 0.75f / size.y);
			if (startIndex >= (totalCount - totalVisibleCount))
				scrollPosition.y += size.y;
		}
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (clearOnNewSceneLoaded)
			clear();

		currentScene = SceneManager.GetActiveScene().name;
	}

	//save user config
	void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("Reporter_currentView", (int)currentView);
		PlayerPrefs.SetInt("Reporter_show", (show == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_collapse", (collapse == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_clearOnNewSceneLoaded", (clearOnNewSceneLoaded == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showTime", (showTime == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showScene", (showScene == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showMemory", (showMemory == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showFps", (showFps == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showGraph", (showGraph == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showLog", (showLog == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showWarning", (showWarning == true) ? 1 : 0);
		PlayerPrefs.SetInt("Reporter_showError", (showError == true) ? 1 : 0);

		PlayerPrefs.Save();
	}

	//read build information 
	IEnumerator readInfo()
	{
		string prefFile = "build_info.txt";
		string url = prefFile;

		if (prefFile.IndexOf("://") == -1)
		{
			string streamingAssetsPath = Application.streamingAssetsPath;
			if (streamingAssetsPath == "")
				streamingAssetsPath = Application.dataPath + "/StreamingAssets/";
			url = System.IO.Path.Combine(streamingAssetsPath, prefFile);
		}

		if (!url.Contains("://"))
			url = "file://" + url;


		// float startTime = Time.realtimeSinceStartup;
		WWW www = new WWW(url);
		yield return www;

		if (!string.IsNullOrEmpty(www.error))
		{
			//Debuger.LogError(www.error);
		}
		else
		{
			buildDate = www.text;
		}

		yield break;
	}

	bool openSpeeder = true;
	public void ActiveCameraOperator()
	{
		// openSpeeder = !openSpeeder;
		//if(openSpeeder == false) {
		//     SpeedOptimized.instance.gameObject.SetActive(false);
		// }
		// else {
		//     SpeedOptimized.instance.gameObject.SetActive(true);
		// }
	}
}

#endif

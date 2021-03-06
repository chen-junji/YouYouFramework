using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

#if UNITY_ADDRESSABLES_EXIST
using UnityEngine.AddressableAssets;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FMODUnity
{
    [AddComponentMenu("")]
    public class RuntimeManager : MonoBehaviour
    {
        static SystemNotInitializedException initException = null;
        static RuntimeManager instance;

        Platform currentPlatform;

        [AOT.MonoPInvokeCallback(typeof(FMOD.DEBUG_CALLBACK))]
        static FMOD.RESULT DEBUG_CALLBACK(FMOD.DEBUG_FLAGS flags, IntPtr filePtr, int line, IntPtr funcPtr, IntPtr messagePtr)
        {
            FMOD.StringWrapper file = new FMOD.StringWrapper(filePtr);
            FMOD.StringWrapper func = new FMOD.StringWrapper(funcPtr);
            FMOD.StringWrapper message = new FMOD.StringWrapper(messagePtr);
            
            if (flags == FMOD.DEBUG_FLAGS.ERROR)
            {
                Debug.LogError(string.Format(("[FMOD] {0} : {1}"), (string)func, (string)message));
            }
            else if (flags == FMOD.DEBUG_FLAGS.WARNING)
            {
                Debug.LogWarning(string.Format(("[FMOD] {0} : {1}"), (string)func, (string)message));
            }
            else if (flags == FMOD.DEBUG_FLAGS.LOG)
            {
                Debug.Log(string.Format(("[FMOD] {0} : {1}"), (string)func, (string)message));
            }
            return FMOD.RESULT.OK;
        }

        static RuntimeManager Instance
        {
            get
            {
                if (initException != null)
                {
                    throw initException;
                }

                if (instance == null)
                {
                    FMOD.RESULT initResult = FMOD.RESULT.OK; // Initialize can return an error code if it falls back to NO_SOUND, throw it as a non-cached exception

                    // When reloading scripts the static instance pointer will be cleared, find the old manager and clean it up
                    foreach (RuntimeManager manager in Resources.FindObjectsOfTypeAll<RuntimeManager>())
                    {
                        DestroyImmediate(manager.gameObject);
                    }

                    var gameObject = new GameObject("FMOD.UnityIntegration.RuntimeManager");
                    instance = gameObject.AddComponent<RuntimeManager>();

                    if (Application.isPlaying) // This class is used in edit mode by the Timeline auditioning system
                    {
                        DontDestroyOnLoad(gameObject);
                    }
                    gameObject.hideFlags = HideFlags.HideAndDontSave;

                    try
                    {
                        #if UNITY_ANDROID && !UNITY_EDITOR
                        // First, obtain the current activity context
                        AndroidJavaObject activity = null;
                        using (var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        {
                            activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                        }

                        using (var fmodJava = new AndroidJavaClass("org.fmod.FMOD"))
                        {
                            if (fmodJava != null)
                            {
                                fmodJava.CallStatic("init", activity);
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("[FMOD] Cannot initialize Java wrapper");
                            }
                        }
                        #endif

                        RuntimeUtils.EnforceLibraryOrder();
                        initResult = instance.Initialize();
                    }
                    catch (Exception e)
                    {
                        initException = e as SystemNotInitializedException;
                        if (initException == null)
                        {
                            initException = new SystemNotInitializedException(e);
                        }
                        throw initException;
                    }

                    if (initResult != FMOD.RESULT.OK)
                    {
                        throw new SystemNotInitializedException(initResult, "Output forced to NO SOUND mode");
                    }
                }
                return instance;
            }
        }

        public static FMOD.Studio.System StudioSystem
        {
            get { return Instance.studioSystem; }
        }

        public static FMOD.System CoreSystem
        {
            get { return Instance.coreSystem; }
        }

        FMOD.Studio.System studioSystem;
        FMOD.System coreSystem;
        FMOD.DSP mixerHead;

        long cachedStudioSystemHandle; // Persists across script reload for cleanup purposes

        struct LoadedBank
        {
            public FMOD.Studio.Bank Bank;
            public int RefCount;
        }

        Dictionary<string, LoadedBank> loadedBanks = new Dictionary<string, LoadedBank>();

        // Explicit comparer to avoid issues on platforms that don't support JIT compilation
        class GuidComparer : IEqualityComparer<Guid>
        {
            bool IEqualityComparer<Guid>.Equals(Guid x, Guid y)
            {
                return x.Equals(y);
            }

            int IEqualityComparer<Guid>.GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
        Dictionary<Guid, FMOD.Studio.EventDescription> cachedDescriptions = new Dictionary<Guid, FMOD.Studio.EventDescription>(new GuidComparer());

        void CheckInitResult(FMOD.RESULT result, string cause)
        {
            if (result != FMOD.RESULT.OK)
            {
                if (studioSystem.isValid())
                {
                    studioSystem.release();
                    studioSystem.clearHandle();
                }
                throw new SystemNotInitializedException(result, cause);
            }
        }

        FMOD.RESULT Initialize()
        {
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
            #endif // UNITY_EDITOR

            FMOD.RESULT result = FMOD.RESULT.OK;
            FMOD.RESULT initResult = FMOD.RESULT.OK;
            Settings fmodSettings = Settings.Instance;
            currentPlatform = fmodSettings.FindCurrentPlatform();

            int sampleRate = currentPlatform.SampleRate;
            int realChannels = Math.Min(currentPlatform.RealChannelCount, 256);
            int virtualChannels = currentPlatform.VirtualChannelCount;
            uint dspBufferLength = (uint)currentPlatform.DSPBufferLength;
            int dspBufferCount = currentPlatform.DSPBufferCount;
            FMOD.SPEAKERMODE speakerMode = currentPlatform.SpeakerMode;
            FMOD.OUTPUTTYPE outputType = currentPlatform.GetOutputType();

            FMOD.ADVANCEDSETTINGS advancedSettings = new FMOD.ADVANCEDSETTINGS();
            advancedSettings.randomSeed = (uint)DateTime.UtcNow.Ticks;
            #if UNITY_EDITOR || UNITY_STANDALONE
            advancedSettings.maxVorbisCodecs = realChannels;
            #elif UNITY_XBOXONE
            advancedSettings.maxXMACodecs = realChannels;
            #elif UNITY_PS4
            advancedSettings.maxAT9Codecs = realChannels;
            #else
            advancedSettings.maxFADPCMCodecs = realChannels;
            #endif

            SetThreadAffinities(currentPlatform);

            currentPlatform.PreSystemCreate(CheckInitResult);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            result = FMOD.Debug.Initialize(fmodSettings.LoggingLevel, FMOD.DEBUG_MODE.CALLBACK, DEBUG_CALLBACK, null);
            if(result == FMOD.RESULT.ERR_UNSUPPORTED)
            {
                Debug.LogWarning("[FMOD] Unable to initialize debug logging: Logging will be disabled.\nCheck the Import Settings of the FMOD libs to enable the logging library.");
            }
            else
            {
                CheckInitResult(result, "FMOD.Debug.Initialize");
            }
            #endif

            FMOD.Studio.INITFLAGS studioInitFlags = FMOD.Studio.INITFLAGS.NORMAL | FMOD.Studio.INITFLAGS.DEFERRED_CALLBACKS;
            if (currentPlatform.IsLiveUpdateEnabled)
            {
                studioInitFlags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
                advancedSettings.profilePort = (ushort)currentPlatform.LiveUpdatePort;
            }

retry:
            result = FMOD.Studio.System.create(out studioSystem);
            CheckInitResult(result, "FMOD.Studio.System.create");
            cachedStudioSystemHandle = (long)studioSystem.handle;

            result = studioSystem.getCoreSystem(out coreSystem);
            CheckInitResult(result, "FMOD.Studio.System.getCoreSystem");

            result = coreSystem.setOutput(outputType);
            CheckInitResult(result, "FMOD.System.setOutput");

            result = coreSystem.setSoftwareChannels(realChannels);
            CheckInitResult(result, "FMOD.System.setSoftwareChannels");

            result = coreSystem.setSoftwareFormat(sampleRate, speakerMode, 0);
            CheckInitResult(result, "FMOD.System.setSoftwareFormat");

            if (dspBufferLength > 0 && dspBufferCount > 0)
            {
                result = coreSystem.setDSPBufferSize(dspBufferLength, dspBufferCount);
                CheckInitResult(result, "FMOD.System.setDSPBufferSize");
            }

            result = coreSystem.setAdvancedSettings(ref advancedSettings);
            CheckInitResult(result, "FMOD.System.setAdvancedSettings");

            if (!string.IsNullOrEmpty(Settings.Instance.EncryptionKey))
            {
                FMOD.Studio.ADVANCEDSETTINGS studioAdvancedSettings = new FMOD.Studio.ADVANCEDSETTINGS();
                result = studioSystem.setAdvancedSettings(studioAdvancedSettings, Settings.Instance.EncryptionKey);
                CheckInitResult(result, "FMOD.Studio.System.setAdvancedSettings");
            }

            if (Settings.Instance.EnableMemoryTracking)
            {
                studioInitFlags |= FMOD.Studio.INITFLAGS.MEMORY_TRACKING;
            }

            currentPlatform.PreInitialize(studioSystem);

            PlatformCallbackHandler callbackHandler = currentPlatform.CallbackHandler;

            if (callbackHandler != null)
            {
                callbackHandler.PreInitialize(studioSystem, CheckInitResult);
            }

            result = studioSystem.initialize(virtualChannels, studioInitFlags, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != FMOD.RESULT.OK && initResult == FMOD.RESULT.OK)
            {
                initResult = result; // Save this to throw at the end (we'll attempt NO SOUND to shield ourselves from unexpected device failures)
                outputType = FMOD.OUTPUTTYPE.NOSOUND;
                UnityEngine.Debug.LogErrorFormat("[FMOD] Studio::System::initialize returned {0}, defaulting to no-sound mode.", result.ToString());

                goto retry;
            }
            CheckInitResult(result, "Studio::System::initialize");

            // Test network functionality triggered during System::update
            if ((studioInitFlags & FMOD.Studio.INITFLAGS.LIVEUPDATE) != 0)
            {
                studioSystem.flushCommands(); // Any error will be returned through Studio.System.update

                result = studioSystem.update();
                if (result == FMOD.RESULT.ERR_NET_SOCKET_ERROR)
                {
                    studioInitFlags &= ~FMOD.Studio.INITFLAGS.LIVEUPDATE;
                    UnityEngine.Debug.LogWarning("[FMOD] Cannot open network port for Live Update (in-use), restarting with Live Update disabled.");

                    result = studioSystem.release();
                    CheckInitResult(result, "FMOD.Studio.System.Release");

                    goto retry;
                }
            }

            currentPlatform.LoadPlugins(coreSystem, CheckInitResult);
            LoadBanks(fmodSettings);

            #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            RegisterSuspendCallback(HandleInterrupt);
            #endif

            return initResult;
        }

        private static void SetThreadAffinities(Platform platform)
        {
            foreach (ThreadAffinityGroup group in platform.ThreadAffinities)
            {
                foreach (ThreadType thread in group.threads)
                {
                    FMOD.THREAD_TYPE fmodThread = RuntimeUtils.ToFMODThreadType(thread);
                    FMOD.THREAD_AFFINITY fmodAffinity = RuntimeUtils.ToFMODThreadAffinity(group.affinity);

                    FMOD.Thread.SetAttributes(fmodThread, fmodAffinity);
                }
            }
        }

        List<StudioEventEmitter> activeEmitters = new List<StudioEventEmitter>();

        class AttachedInstance
        {
            public FMOD.Studio.EventInstance instance;
            public Transform transform;
            public Rigidbody rigidBody;
            public Rigidbody2D rigidBody2D;
        }

        List<AttachedInstance> attachedInstances = new List<AttachedInstance>(128);

        #if UNITY_EDITOR
        List<FMOD.Studio.EventInstance> eventPositionWarnings = new List<FMOD.Studio.EventInstance>();
        #endif

        public static int AddListener(StudioListener listener)
        {
            // Is the listener already in the list?
            for (int i = 0; i < Listeners.Count; i++)
            {
                if (Listeners[i] != null && listener.gameObject == Listeners[i].gameObject)
                {
                    Debug.LogWarning(string.Format(("[FMOD] Listener has already been added at index {0}."), i));
                    return i;
                }
            }
            // If already at the max numListeners
            if (numListeners >= FMOD.CONSTANTS.MAX_LISTENERS)
            {
                Debug.LogWarning(string.Format(("[FMOD] Max number of listeners reached : {0}."), FMOD.CONSTANTS.MAX_LISTENERS));
                //return -1;
            }

            // If not already in the list
            // The next available spot in the list should be at `numListeners`
            if (Listeners.Count <= numListeners)
            {
                Listeners.Add(listener);
            }
            else
            {
                Listeners[numListeners] = listener;
            }
            // Increment `numListeners`
            numListeners++;
            // setNumListeners (8 is the most that FMOD supports)
            int numListenersClamped = Mathf.Min(numListeners, FMOD.CONSTANTS.MAX_LISTENERS);
            StudioSystem.setNumListeners(numListenersClamped);
            return numListeners - 1;
        }

        public static bool RemoveListener(StudioListener listener)
        {
            int index = listener.ListenerNumber;
            // Remove listener
            if (index != -1)
            {
                Listeners[index] = null;

                // Are there more listeners above the index of the one we are removing?
                if (numListeners - 1 > index)
                {
                    // Move any higher index listeners down
                    for (int i = index; i < Listeners.Count; i++)
                    {
                        if (i == Listeners.Count - 1)
                        {
                            Listeners[i] = null;
                        }
                        else
                        {
                            Listeners[i] = Listeners[i + 1];
                            if (Listeners[i])
                            {
                                Listeners[i].ListenerNumber = i;
                            }
                        }
                    }
                }
                // Decriment numListeners
                numListeners--;
                // Always need at least 1 listener, otherwise "[FMOD] assert : assertion: 'numListeners >= 1 && numListeners <= 8' failed"
                int numListenersClamped = Mathf.Min(Mathf.Max(numListeners, 1), FMOD.CONSTANTS.MAX_LISTENERS);
                StudioSystem.setNumListeners(numListenersClamped);
                // Listener attributes will be updated before the next update, due to the Script Execution Order.
                return true;
            }
            else
            {
                return false;
            }
        }

        bool listenerWarningIssued = false;

        void Update()
        {
            if (studioSystem.isValid())
            {
                if (numListeners <= 0 && !listenerWarningIssued)
                {
                    listenerWarningIssued = true;
                    UnityEngine.Debug.LogWarning("[FMOD] Please add an 'FMOD Studio Listener' component to your a camera in the scene for correct 3D positioning of sounds.");
                }

                for (int i = 0; i < activeEmitters.Count; i++)
                {
                    UpdateActiveEmitter(activeEmitters[i]);
                }

                for (int i = 0; i < attachedInstances.Count; i++)
                {
                    FMOD.Studio.PLAYBACK_STATE playbackState = FMOD.Studio.PLAYBACK_STATE.STOPPED;
                    attachedInstances[i].instance.getPlaybackState(out playbackState);
                    if (!attachedInstances[i].instance.isValid() || 
                        playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED ||
                        attachedInstances[i].transform == null // destroyed game object
                        )
                    {
                        attachedInstances.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (attachedInstances[i].rigidBody)
                    {
                        attachedInstances[i].instance.set3DAttributes(RuntimeUtils.To3DAttributes(attachedInstances[i].transform, attachedInstances[i].rigidBody));
                    }
                    else
                    {
                        attachedInstances[i].instance.set3DAttributes(RuntimeUtils.To3DAttributes(attachedInstances[i].transform, attachedInstances[i].rigidBody2D));
                    }
                }

                #if UNITY_EDITOR
                MuteAllEvents(UnityEditor.EditorUtility.audioMasterMute);

                for (int i = eventPositionWarnings.Count - 1; i >= 0; i--)
                {
                    if (eventPositionWarnings[i].isValid())
                    {
                        FMOD.ATTRIBUTES_3D attribs;
                        eventPositionWarnings[i].get3DAttributes(out attribs);
                        if (attribs.position.x == 1e+18F &&
                            attribs.position.y == 1e+18F &&
                            attribs.position.z == 1e+18F)
                        {
                            string path;
                            FMOD.Studio.EventDescription desc;
                            eventPositionWarnings[i].getDescription(out desc);
                            desc.getPath(out path);
                            Debug.LogWarningFormat("[FMOD] Instance of Event {0} has not had EventInstance.set3DAttributes() called on it yet!", path);
                        }
                    }
                    eventPositionWarnings.RemoveAt(i);
                }

                isOverlayEnabled = currentPlatform.IsOverlayEnabled;
                #endif

                if (isOverlayEnabled)
                {
                    if (!overlayDrawer)
                    {
                        overlayDrawer = Instance.gameObject.AddComponent<FMODRuntimeManagerOnGUIHelper>();
                        overlayDrawer.TargetRuntimeManager = this;
                    }
                    else
                    {
                        overlayDrawer.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (overlayDrawer != null && overlayDrawer.gameObject.activeSelf)
                    {
                        overlayDrawer.gameObject.SetActive(false);
                    }
                }

                studioSystem.update();
            }
        }

        public static void RegisterActiveEmitter(StudioEventEmitter emitter)
        {
            if (!Instance.activeEmitters.Contains(emitter))
            {
                Instance.activeEmitters.Add(emitter);
            }
        }

        public static void DeregisterActiveEmitter(StudioEventEmitter emitter)
        {
            Instance.activeEmitters.Remove(emitter);
        }

        public static void UpdateActiveEmitter(StudioEventEmitter emitter, bool force = false)
        {
            // If at least once listener is within the max distance, ensure an event instance is playing
            bool playInstance = false;
            for (int i = 0; i < Listeners.Count; i++)
            {
                if (Vector3.Distance(emitter.transform.position, Listeners[i].transform.position) <= emitter.MaxDistance)
                {
                    playInstance = true;
                    break;
                }
            }
            
            if (force || playInstance != emitter.IsPlaying())
            {
                if (playInstance)
                {
                    emitter.PlayInstance();
                }
                else
                {
                    emitter.StopInstance();
                }
            }
        }

        public static void AttachInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, Rigidbody rigidBody)
        {
            AttachedInstance attachedInstance = Instance.attachedInstances.Find(x => x.instance.handle == instance.handle);
            if (attachedInstance == null)
            {
                attachedInstance = new AttachedInstance();
                Instance.attachedInstances.Add(attachedInstance);
            }

            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform, rigidBody));
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;
            attachedInstance.rigidBody = rigidBody;
        }

        public static void AttachInstanceToGameObject(FMOD.Studio.EventInstance instance, Transform transform, Rigidbody2D rigidBody2D)
        {
            AttachedInstance attachedInstance = Instance.attachedInstances.Find(x => x.instance.handle == instance.handle);
            if (attachedInstance == null)
            {
                attachedInstance = new AttachedInstance();
                Instance.attachedInstances.Add(attachedInstance);
            }

            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform, rigidBody2D));
            attachedInstance.transform = transform;
            attachedInstance.instance = instance;
            attachedInstance.rigidBody2D = rigidBody2D;
            attachedInstance.rigidBody = null;
        }

        public static void DetachInstanceFromGameObject(FMOD.Studio.EventInstance instance)
        {
            var manager = Instance;
            for (int i = 0; i < manager.attachedInstances.Count; i++)
            {
                if (manager.attachedInstances[i].instance.handle == instance.handle)
                {
                    manager.attachedInstances.RemoveAt(i);
                    return;
                }
            }
        }

        protected bool isOverlayEnabled = false;
        FMODRuntimeManagerOnGUIHelper overlayDrawer = null;
        Rect windowRect = new Rect(10, 10, 300, 100);

        public void ExecuteOnGUI()
        {
            if (studioSystem.isValid() && isOverlayEnabled)
            {
                windowRect = GUI.Window(GetInstanceID(), windowRect, DrawDebugOverlay, "FMOD Studio Debug");
            }
        }

        #if !UNITY_EDITOR
        private void Start()
        {
            isOverlayEnabled = currentPlatform.IsOverlayEnabled;
        }
        #endif

        string lastDebugText;
        float lastDebugUpdate = 0;
        void DrawDebugOverlay(int windowID)
        {
            if (lastDebugUpdate + 0.25f < Time.unscaledTime)
            {
                if (initException != null)
                {
                    lastDebugText = initException.Message;
                }
                else
                {
                    if (!mixerHead.hasHandle())
                    {
                        FMOD.ChannelGroup master;
                        coreSystem.getMasterChannelGroup(out master);
                        master.getDSP(0, out mixerHead);
                        mixerHead.setMeteringEnabled(false, true);
                    }

                    StringBuilder debug = new StringBuilder();

                    FMOD.Studio.CPU_USAGE cpuUsage;
                    studioSystem.getCPUUsage(out cpuUsage);
                    debug.AppendFormat("CPU: dsp = {0:F1}%, studio = {1:F1}%\n", cpuUsage.dspusage, cpuUsage.studiousage);

                    int currentAlloc, maxAlloc;
                    FMOD.Memory.GetStats(out currentAlloc, out maxAlloc);
                    debug.AppendFormat("MEMORY: cur = {0}MB, max = {1}MB\n", currentAlloc >> 20, maxAlloc >> 20);

                    int realchannels, channels;
                    coreSystem.getChannelsPlaying(out channels, out realchannels);
                    debug.AppendFormat("CHANNELS: real = {0}, total = {1}\n", realchannels, channels);

                    FMOD.DSP_METERING_INFO outputMetering;
                    mixerHead.getMeteringInfo(IntPtr.Zero, out outputMetering);
                    float rms = 0;
                    for (int i = 0; i < outputMetering.numchannels; i++)
                    {
                        rms += outputMetering.rmslevel[i] * outputMetering.rmslevel[i];
                    }
                    rms = Mathf.Sqrt(rms / (float)outputMetering.numchannels);

                    float db = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
                    if (db > 10.0f) db = 10.0f;

                    debug.AppendFormat("VOLUME: RMS = {0:f2}db\n", db);
                    lastDebugText = debug.ToString();
                    lastDebugUpdate = Time.unscaledTime;
                }
            }

            GUI.Label(new Rect(10, 20, 290, 100), lastDebugText);
            GUI.DragWindow();
        }

        void OnDestroy()
        {
            if (!studioSystem.isValid())
            {
                studioSystem.handle = (IntPtr)cachedStudioSystemHandle;
            }

            if (studioSystem.isValid())
            {
                studioSystem.release();
            }

            initException = null;
            instance = null;
        }

        #if UNITY_EDITOR
        public static void Destroy()
        {
            if (instance)
            {
                DestroyImmediate(instance.gameObject);
            }
        }

        void HandlePlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredEditMode)
            {
                Destroy();
            }
        }
        #endif

        #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        [AOT.MonoPInvokeCallback(typeof(Action<bool>))]
        static void HandleInterrupt(bool began)
        {
            if (Instance.studioSystem.isValid())
            {
                // Strings bank is always loaded
                if (Instance.loadedBanks.Count > 1)
                    PauseAllEvents(began);

                if (began)
                {
                    Instance.coreSystem.mixerSuspend();
                }
                else
                {
                    Instance.coreSystem.mixerResume();
                }
            }
        }
        #else
        void OnApplicationPause(bool pauseStatus)
        {
            if (studioSystem.isValid())
            {
                PauseAllEvents(pauseStatus);

                if (pauseStatus)
                {
                    coreSystem.mixerSuspend();
                }
                else
                {
                    coreSystem.mixerResume();
                }
            }
        }
        #endif

        private void loadedBankRegister(LoadedBank loadedBank, string bankPath, string bankName, bool loadSamples, FMOD.RESULT loadResult)
        {
            if (loadResult == FMOD.RESULT.OK)
            {
                loadedBank.RefCount = 1;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }

                Instance.loadedBanks.Add(bankName, loadedBank);
            }
            else if (loadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                // someone loaded this bank directly using the studio API
                // TODO: will the null bank handle be an issue
                loadedBank.RefCount = 2;
                Instance.loadedBanks.Add(bankName, loadedBank);
            }
            else
            {
                throw new BankLoadException(bankPath, loadResult);
            }
        }

#if UNITY_ANDROID || UNITY_WEBGL
        IEnumerator loadFromWeb(string bankPath, string bankName, bool loadSamples)
        {
            byte[] loadWebResult;
            FMOD.RESULT loadResult;

            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(bankPath);
            yield return www.SendWebRequest();
            loadWebResult = www.downloadHandler.data;

            LoadedBank loadedBank = new LoadedBank();
            loadResult = Instance.studioSystem.loadBankMemory(loadWebResult, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
            if (loadResult != FMOD.RESULT.OK)
            {
                UnityEngine.Debug.LogWarningFormat("[FMOD] loadFromWeb.  Path = {0}, result = {1}.", bankPath, loadResult);
            }
            loadedBankRegister(loadedBank, bankPath, bankName, loadSamples, loadResult);

            Debug.LogFormat("[FMOD] Finished loading {0}", bankPath);
        }
#endif // UNITY_ANDROID || UNITY_WEBGL

        public static void LoadBank(string bankName, bool loadSamples = false)
        {
            if (Instance.loadedBanks.ContainsKey(bankName))
            {
                LoadedBank loadedBank = Instance.loadedBanks[bankName];
                loadedBank.RefCount++;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }
                Instance.loadedBanks[bankName] = loadedBank;
            }
            else
            {
                string bankFolder = Instance.currentPlatform.GetBankFolder();

#if !UNITY_EDITOR
                if (!string.IsNullOrEmpty(Settings.Instance.TargetSubFolder))
                {
                    bankFolder = Path.Combine(bankFolder, Settings.Instance.TargetSubFolder);
                }
#endif

                const string BankExtension = ".bank";

                string bankPath;

                if (System.IO.Path.GetExtension(bankName) != BankExtension)
                {
                    bankPath = string.Format("{0}/{1}{2}", bankFolder, bankName, BankExtension);
                }
                else
                {
                    bankPath = string.Format("{0}/{1}", bankFolder, bankName);
                }

                #if UNITY_ANDROID && !UNITY_EDITOR 
                if (Settings.Instance.AndroidUseOBB)
                {
                    #if UNITY_2018_1_OR_NEWER
                    Instance.StartCoroutine(Instance.loadFromWeb(bankPath, bankName, loadSamples));
                    #else
                    using (var www = new WWW(bankPath))
                    {
                        while (!www.isDone) { }
                        if (!String.IsNullOrEmpty(www.error))
                        {
                            throw new BankLoadException(bankPath, www.error);
                        }
                        else
                        {
                            LoadedBank loadedBank = new LoadedBank();
                            FMOD.RESULT loadResult = Instance.studioSystem.loadBankMemory(www.bytes, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                            Instance.loadedBankRegister(loadedBank, bankPath, bankName, loadSamples, loadResult);
                        }
                    }
                    #endif
                }
                else
                #elif UNITY_WEBGL && !UNITY_EDITOR
                if (true)
                {
                    Instance.StartCoroutine(Instance.loadFromWeb(bankPath, bankName, loadSamples));
                }
                else
                #endif // (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
                {
                    LoadedBank loadedBank = new LoadedBank();
                    FMOD.RESULT loadResult = Instance.studioSystem.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
                    Instance.loadedBankRegister(loadedBank, bankPath, bankName, loadSamples, loadResult);
                }
            }
        }

        public static void LoadBank(TextAsset asset, bool loadSamples = false)
        {
            string bankName = asset.name;
            if (Instance.loadedBanks.ContainsKey(bankName))
            {
                LoadedBank loadedBank = Instance.loadedBanks[bankName];
                loadedBank.RefCount++;

                if (loadSamples)
                {
                    loadedBank.Bank.loadSampleData();
                }
            }
            else
            {
                LoadedBank loadedBank = new LoadedBank();
                FMOD.RESULT loadResult = Instance.studioSystem.loadBankMemory(asset.bytes, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);

                if (loadResult == FMOD.RESULT.OK)
                {
                    loadedBank.RefCount = 1;
                    Instance.loadedBanks.Add(bankName, loadedBank);

                    if (loadSamples)
                    {
                        loadedBank.Bank.loadSampleData();
                    }
                }
                else if (loadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
                {
                    // someone loaded this bank directly using the studio API
                    // TODO: will the null bank handle be an issue
                    loadedBank.RefCount = 2;
                    Instance.loadedBanks.Add(bankName, loadedBank);
                }
                else
                {
                    throw new BankLoadException(bankName, loadResult);
                }
            }
        }

        #if UNITY_ADDRESSABLES_EXIST
        public static void LoadBank(AssetReference assetReference, bool loadSamples = false, System.Action completionCallback = null)
        {
            if (loadSamples || completionCallback != null)
            {
                assetReference.LoadAssetAsync<TextAsset>().Completed += (result) =>
                {
                    Asset_Completed(result, loadSamples);

                    if (completionCallback != null)
                    {
                        completionCallback();
                    }
                };
            }
            else
            {
                assetReference.LoadAssetAsync<TextAsset>().Completed += Asset_Completed;
            }
        }

        private static void Asset_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> obj)
        {
            Asset_Completed(obj, false);
        }

        private static void Asset_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> obj, bool loadSamples)
        {
            if (!obj.IsValid())
            {
                Debug.LogError("[FMOD] Unable to load AssetReference: " + obj.OperationException);
                return;
            }

            TextAsset bank = obj.Result;
            LoadBank(bank, loadSamples);
        }
        #endif

        private void LoadBanks(Settings fmodSettings)
        {
            if (fmodSettings.ImportType == ImportType.StreamingAssets)
            {
                // Always load strings bank
                try
                {
                    switch (fmodSettings.BankLoadType)
                    {
                        case BankLoadType.All:
                            foreach (string masterBankFileName in fmodSettings.MasterBanks)
                            {
                                LoadBank(masterBankFileName + ".strings", fmodSettings.AutomaticSampleLoading);
                                LoadBank(masterBankFileName, fmodSettings.AutomaticSampleLoading);
                            }

                            foreach (var bank in fmodSettings.Banks)
                            {
                                LoadBank(bank, fmodSettings.AutomaticSampleLoading);
                            }

                            WaitForAllLoads();
                            break;
                        case BankLoadType.Specified:
                            foreach (var bank in fmodSettings.BanksToLoad)
                            {
                                if (!string.IsNullOrEmpty(bank))
                                {
                                    LoadBank(bank, fmodSettings.AutomaticSampleLoading);
                                }
                            }

                            WaitForAllLoads();
                            break;
                        case BankLoadType.None:
                            break;
                        default:
                            break;
                    }
                }
                catch (BankLoadException e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public static void UnloadBank(string bankName)
        {
            LoadedBank loadedBank;
            if (Instance.loadedBanks.TryGetValue(bankName, out loadedBank))
            {
                loadedBank.RefCount--;
                if (loadedBank.RefCount == 0)
                {
                    loadedBank.Bank.unload();
                    Instance.loadedBanks.Remove(bankName);
                    return;
                }
                Instance.loadedBanks[bankName] = loadedBank;
            }
        }

        public static bool AnyBankLoading()
        {
            bool loading = false;
            foreach (LoadedBank bank in Instance.loadedBanks.Values)
            {
                FMOD.Studio.LOADING_STATE loadingState;
                bank.Bank.getSampleLoadingState(out loadingState);
                loading |= (loadingState == FMOD.Studio.LOADING_STATE.LOADING);
            }
            return loading;
        }

        public static void WaitForAllLoads()
        {
            Instance.studioSystem.flushSampleLoading();
        }

        public static Guid PathToGUID(string path)
        {
            Guid guid = Guid.Empty;
            if (path.StartsWith("{"))
            {
                FMOD.Studio.Util.parseID(path, out guid);
            }
            else
            {
                var result = Instance.studioSystem.lookupID(path, out guid);
                if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
                {
                    throw new EventNotFoundException(path);
                }
            }
            return guid;
        }

        public static FMOD.Studio.EventInstance CreateInstance(string path)
        {
            try
            {
                return CreateInstance(PathToGUID(path));
            }
            catch(EventNotFoundException)
            {
                // Switch from exception with GUID to exception with path
                throw new EventNotFoundException(path);
            }
        }

        public static FMOD.Studio.EventInstance CreateInstance(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc = GetEventDescription(guid);
            FMOD.Studio.EventInstance newInstance;
            eventDesc.createInstance(out newInstance);

            #if UNITY_EDITOR
            bool is3D = false;
            eventDesc.is3D(out is3D);
            if (is3D)
            {
                // Set position to 1e+18F, set3DAttributes should be called by the dev after this.
                newInstance.set3DAttributes(RuntimeUtils.To3DAttributes(new Vector3(1e+18F, 1e+18F, 1e+18F)));
                instance.eventPositionWarnings.Add(newInstance);
            }
            #endif

            return newInstance;
        }

        public static void PlayOneShot(string path, Vector3 position = new Vector3())
        {
            try
            {
                PlayOneShot(PathToGUID(path), position);
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning("[FMOD] Event not found: " + path);
            }
        }

        public static void PlayOneShot(Guid guid, Vector3 position = new Vector3())
        {
            var instance = CreateInstance(guid);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            instance.release();
        }

        public static void PlayOneShotAttached(string path, GameObject gameObject)
        {
            try
            {
                PlayOneShotAttached(PathToGUID(path), gameObject);
            }
            catch (EventNotFoundException)
            {
                Debug.LogWarning("[FMOD] Event not found: " + path);
            }
        }

        public static void PlayOneShotAttached(Guid guid, GameObject gameObject)
        {
            var instance = CreateInstance(guid);
            AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
            instance.start();
            instance.release();
        }

        public static FMOD.Studio.EventDescription GetEventDescription(string path)
        {
            try
            {
                return GetEventDescription(PathToGUID(path));
            }
            catch (EventNotFoundException)
            {
                throw new EventNotFoundException(path);
            }
        }

        public static FMOD.Studio.EventDescription GetEventDescription(Guid guid)
        {
            FMOD.Studio.EventDescription eventDesc;
            if (Instance.cachedDescriptions.ContainsKey(guid) && Instance.cachedDescriptions[guid].isValid())
            {
                eventDesc = Instance.cachedDescriptions[guid];
            }
            else
            {
                var result = Instance.studioSystem.getEventByID(guid, out eventDesc);

                if (result != FMOD.RESULT.OK)
                {
                    throw new EventNotFoundException(guid);
                }

                if (eventDesc.isValid())
                {
                    Instance.cachedDescriptions[guid] = eventDesc;
                }
            }
            return eventDesc;
        }

        public static List<StudioListener> Listeners = new List<StudioListener>();
        private static int numListeners = 0;

        public static void SetListenerLocation(GameObject gameObject, Rigidbody rigidBody = null, GameObject attenuationObject = null)
        {
            SetListenerLocation3D(0, gameObject.transform, rigidBody, attenuationObject);
        }
        
        public static void SetListenerLocation(GameObject gameObject, Rigidbody2D rigidBody2D, GameObject attenuationObject = null)
        {
            SetListenerLocation2D(0, gameObject.transform, rigidBody2D, attenuationObject);
        }

        public static void SetListenerLocation(Transform transform, GameObject attenuationObject = null)
        {
            SetListenerLocation3D(0, transform, null, attenuationObject);
        }

        public static void SetListenerLocation(int listenerIndex, GameObject gameObject, Rigidbody rigidBody = null, GameObject attenuationObject = null)
        {
            SetListenerLocation3D(listenerIndex, gameObject.transform, rigidBody, attenuationObject);
        }
        
        public static void SetListenerLocation(int listenerIndex, GameObject gameObject, Rigidbody2D rigidBody2D, GameObject attenuationObject = null)
        {
            SetListenerLocation2D(listenerIndex, gameObject.transform, rigidBody2D, attenuationObject);
        }

        public static void SetListenerLocation(int listenerIndex, Transform transform, GameObject attenuationObject = null)
        {
            SetListenerLocation3D(0, transform, null, attenuationObject);
        }

        private static void SetListenerLocation3D(int listenerIndex, Transform transform, Rigidbody rigidBody = null, GameObject attenuationObject = null)
        {
            if (attenuationObject)
            {
                Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(transform, rigidBody), RuntimeUtils.ToFMODVector(attenuationObject.transform.position));
            }
            else
            {
                Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(transform, rigidBody));
            }
        }

        private static void SetListenerLocation2D(int listenerIndex, Transform transform, Rigidbody2D rigidBody = null, GameObject attenuationObject = null)
        {
            if (attenuationObject)
            {
                Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(transform, rigidBody), RuntimeUtils.ToFMODVector(attenuationObject.transform.position));
            }
            else
            {
                Instance.studioSystem.setListenerAttributes(0, RuntimeUtils.To3DAttributes(transform, rigidBody));
            }
        }

        public static FMOD.Studio.Bus GetBus(string path)
        {
            FMOD.Studio.Bus bus;
            if (StudioSystem.getBus(path, out bus) != FMOD.RESULT.OK)
            {
                throw new BusNotFoundException(path);
            }
            return bus;
        }

        public static FMOD.Studio.VCA GetVCA(string path)
        {
            FMOD.Studio.VCA vca;
            if (StudioSystem.getVCA(path, out vca) != FMOD.RESULT.OK)
            {
                throw new VCANotFoundException(path);
            }
            return vca;
        }

        public static void PauseAllEvents(bool paused)
        {
            if (HasBanksLoaded)
            {
                FMOD.Studio.Bus masterBus;
                if (StudioSystem.getBus("bus:/", out masterBus) == FMOD.RESULT.OK)
                {
                    masterBus.setPaused(paused);
                }
            }
        }

        public static void MuteAllEvents(bool muted)
        {
            if (HasBanksLoaded)
            {
                FMOD.Studio.Bus masterBus;
                if (StudioSystem.getBus("bus:/", out masterBus) == FMOD.RESULT.OK)
                {
                    masterBus.setMute(muted);
                }
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return instance != null && instance.studioSystem.isValid();
            }
        }

        public static bool HasBanksLoaded
        {
            get
            {
                return Instance.loadedBanks.Count > 1; 
            }
        }

        public static bool HasBankLoaded(string loadedBank)
        {
            return (Instance.loadedBanks.ContainsKey(loadedBank));
        }

        #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RegisterSuspendCallback(Action<bool> func);
        #endif
    }
}
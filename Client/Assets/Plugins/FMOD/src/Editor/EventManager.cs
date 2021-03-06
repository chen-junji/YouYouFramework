using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using System.Reflection;
using System.Collections;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace FMODUnity
{
    [InitializeOnLoad]
    public class EventManager : MonoBehaviour
    {
        const string CacheAssetName = "FMODStudioCache";
        const string CacheAssetFullName = "Assets/Plugins/FMOD/Cache/Editor/" + CacheAssetName + ".asset";
        static EventCache eventCache;

        const string StringBankExtension = "strings.bank";
        const string BankExtension = "bank";

        const int FilePollTimeSeconds = 5;

        // How many seconds to wait since last file activity to start the import
        const int CountdownTimerReset = 15 / FilePollTimeSeconds;

        static int countdownTimer;

#if UNITY_EDITOR
        [MenuItem("FMOD/Refresh Banks", priority = 1)]
        public static void ForceRefresh()
        {
            countdownTimer = 0;
            RefreshBanks();
        }

        public static void RefreshBanks()
        {
            UpdateCache();
            OnCacheChange();
            if (Settings.Instance.ImportType == ImportType.AssetBundle)
            {
                CopyToStreamingAssets();
            }
        }
#endif

        static void ClearCache()
        {
            countdownTimer = CountdownTimerReset;
            eventCache.StringsBankWriteTime = DateTime.MinValue;
            eventCache.EditorBanks.Clear();
            eventCache.EditorEvents.Clear();
            eventCache.EditorParameters.Clear();
            eventCache.StringsBanks.Clear();
            eventCache.MasterBanks.Clear();
            if (Settings.Instance && Settings.Instance.BanksToLoad != null)
                Settings.Instance.BanksToLoad.Clear();
        }

        static public void UpdateCache()
        {
            // Deserialize the cache from the unity resources
            if (eventCache == null)
            {
                eventCache = AssetDatabase.LoadAssetAtPath(CacheAssetFullName, typeof(EventCache)) as EventCache;
                if (eventCache == null || eventCache.cacheVersion != EventCache.CurrentCacheVersion)
                {
                    UnityEngine.Debug.Log("FMOD Studio: Cannot find serialized event cache or cache in old format, creating new instance");
                    eventCache = ScriptableObject.CreateInstance<EventCache>();
                    eventCache.cacheVersion = EventCache.CurrentCacheVersion;

                    Directory.CreateDirectory(Path.GetDirectoryName(CacheAssetFullName));
                    AssetDatabase.CreateAsset(eventCache, CacheAssetFullName);
                }
            }

            var settings = Settings.Instance;

            if (string.IsNullOrEmpty(settings.SourceBankPath))
            {
                ClearCache();
                return;
            }

            string defaultBankFolder = null;

            if (!settings.HasPlatforms)
            {
                defaultBankFolder = settings.SourceBankPath;
            }
            else
            {
                Platform platform = settings.CurrentEditorPlatform;

                if (platform == settings.DefaultPlatform)
                {
                    platform = settings.PlayInEditorPlatform;
                }

                defaultBankFolder = RuntimeUtils.GetCommonPlatformPath(Path.Combine(settings.SourceBankPath, platform.BuildDirectory));
            }

            string[] bankPlatforms = EditorUtils.GetBankPlatforms();
            string[] bankFolders = new string[bankPlatforms.Length];
            for (int i = 0; i < bankPlatforms.Length; i++)
            {
                bankFolders[i] = RuntimeUtils.GetCommonPlatformPath(Path.Combine(settings.SourceBankPath, bankPlatforms[i]));
            }

            List<string> stringBanks = new List<string>(0);
            try
            {
                var files = Directory.GetFiles(defaultBankFolder, "*." + StringBankExtension, SearchOption.AllDirectories);
                stringBanks = new List<string>(files);
            }
            catch
            {
            }

            // Strip out OSX resource-fork files that appear on FAT32
            stringBanks.RemoveAll((x) => Path.GetFileName(x).StartsWith("._"));

            if (stringBanks.Count == 0)
            {
                bool wasValid = eventCache.StringsBankWriteTime != DateTime.MinValue;
                ClearCache();
                if (wasValid)
                {
                    UnityEngine.Debug.LogError(string.Format("FMOD Studio: Directory {0} doesn't contain any banks. Build the banks in Studio or check the path in the settings.", defaultBankFolder));
                }
                return;
            }

            // If we have multiple .strings.bank files find the most recent
            stringBanks.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            // Use the most recent string bank timestamp as a marker for the most recent build of any bank because it gets exported every time
            DateTime lastWriteTime = File.GetLastWriteTime(stringBanks[0]);

            if (lastWriteTime == eventCache.StringsBankWriteTime)
            {
                countdownTimer = CountdownTimerReset;
                return;
            }

            if (EditorUtils.IsFileOpenByStudio(stringBanks[0]))
            {
                countdownTimer = CountdownTimerReset;
                return;
            }

            // Most recent strings bank is newer than last cache update time, recache.

            // Get a list of all banks
            List<string> bankFileNames = new List<string>();
            List<string> reducedStringBanksList = new List<string>();
            HashSet<Guid> stringBankGuids = new HashSet<Guid>();

            foreach (string stringBankPath in stringBanks)
            {
                FMOD.Studio.Bank stringBank;
                EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));

                if (!stringBank.isValid())
                {
                    countdownTimer = CountdownTimerReset;
                    return;
                }
                else
                {
                    // Unload the strings bank
                    stringBank.unload();
                }
                Guid stringBankGuid;
                EditorUtils.CheckResult(stringBank.getID(out stringBankGuid));

                if (!stringBankGuids.Add(stringBankGuid))
                {
                    // If we encounter multiple string banks with the same GUID then only use the first. This handles the scenario where
                    // a Studio project is cloned and extended for DLC with a new master bank name.
                    continue;
                }

                reducedStringBanksList.Add(stringBankPath);
            }

            bankFileNames = new List<string>(Directory.GetFiles(defaultBankFolder, "*.bank", SearchOption.AllDirectories));
            bankFileNames.RemoveAll(x => x.Contains(".strings"));

            stringBanks = reducedStringBanksList;

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                // Check if any of the files are still being written by studio
                foreach (string bankFileName in bankFileNames)
                {
                    EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => RuntimeUtils.GetCommonPlatformPath(bankFileName) == x.Path);
                    if (bankRef == null)
                    {
                        if (EditorUtils.IsFileOpenByStudio(bankFileName))
                        {
                            countdownTimer = CountdownTimerReset;
                            return;
                        }
                        continue;
                    }

                    if (bankRef.LastModified != File.GetLastWriteTime(bankFileName))
                    {
                        if (EditorUtils.IsFileOpenByStudio(bankFileName))
                        {
                            countdownTimer = CountdownTimerReset;
                            return;
                        }
                    }
                }

                // Count down the timer in case we catch studio in-between updating two files.
                if (countdownTimer-- > 0)
                {
                    return;
                }
            }

            eventCache.StringsBankWriteTime = lastWriteTime;

            // All files are finished being modified by studio so update the cache

            // Stop editor preview so no stale data being held
            EditorUtils.PreviewStop();

            // Reload the strings banks
            List<FMOD.Studio.Bank> loadedStringsBanks = new List<FMOD.Studio.Bank>();

            try
            {
                AssetDatabase.StartAssetEditing();

                eventCache.EditorBanks.ForEach((x) => x.Exists = false);
                HashSet<string> masterBankFileNames = new HashSet<string>();

                foreach (string stringBankPath in stringBanks)
                {
                    FMOD.Studio.Bank stringBank;
                    EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));

                    if (!stringBank.isValid())
                    {
                        ClearCache();
                        return;
                    }

                    loadedStringsBanks.Add(stringBank);

                    FileInfo stringBankFileInfo = new FileInfo(stringBankPath);

                    string masterBankFileName = Path.GetFileName(stringBankPath).Replace(StringBankExtension, BankExtension);
                    masterBankFileNames.Add(masterBankFileName);

                    EditorBankRef stringsBankRef = eventCache.StringsBanks.Find(x => RuntimeUtils.GetCommonPlatformPath(stringBankPath) == x.Path);

                    if (stringsBankRef == null)
                    {
                        stringsBankRef = ScriptableObject.CreateInstance<EditorBankRef>();
                        stringsBankRef.FileSizes = new List<EditorBankRef.NameValuePair>();
                        AssetDatabase.AddObjectToAsset(stringsBankRef, eventCache);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stringsBankRef));
                        eventCache.EditorBanks.Add(stringsBankRef);
                        eventCache.StringsBanks.Add(stringsBankRef);
                    }

                    stringsBankRef.SetPath(stringBankPath, defaultBankFolder);
                    string studioPath;
                    stringBank.getPath(out studioPath);
                    stringsBankRef.SetStudioPath(studioPath);
                    stringsBankRef.LastModified = stringBankFileInfo.LastWriteTime;
                    stringsBankRef.Exists = true;
                    stringsBankRef.FileSizes.Clear();
                  
                    if (Settings.Instance.HasPlatforms)
                    {
                        for (int i = 0; i < bankPlatforms.Length; i++)
                        {
                            stringsBankRef.FileSizes.Add(new EditorBankRef.NameValuePair(bankPlatforms[i], stringBankFileInfo.Length));
                        }
                    }
                    else
                    {
                        stringsBankRef.FileSizes.Add(new EditorBankRef.NameValuePair("", stringBankFileInfo.Length));
                    }
                }

                eventCache.EditorParameters.ForEach((x) => x.Exists = false);
                foreach (string bankFileName in bankFileNames)
                {
                    EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => RuntimeUtils.GetCommonPlatformPath(bankFileName) == x.Path);

                    // New bank we've never seen before
                    if (bankRef == null)
                    {
                        bankRef = ScriptableObject.CreateInstance<EditorBankRef>();
                        AssetDatabase.AddObjectToAsset(bankRef, eventCache);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(bankRef));

                        bankRef.SetPath(bankFileName, defaultBankFolder);
                        bankRef.LastModified = DateTime.MinValue;
                        bankRef.FileSizes = new List<EditorBankRef.NameValuePair>();

                        eventCache.EditorBanks.Add(bankRef);
                    }

                    bankRef.Exists = true;

                    FileInfo bankFileInfo = new FileInfo(bankFileName);

                    // Timestamp check - if it doesn't match update events from that bank
                    if (bankRef.LastModified != bankFileInfo.LastWriteTime)
                    {
                        bankRef.LastModified = bankFileInfo.LastWriteTime;
                        UpdateCacheBank(bankRef);
                    }

                    // Update file sizes
                    bankRef.FileSizes.Clear();
                    if (Settings.Instance.HasPlatforms)
                    {
                        for (int i = 0; i < bankPlatforms.Length; i++)
                        {
                            string platformBankPath = RuntimeUtils.GetCommonPlatformPath(Path.Combine(bankFolders[i], bankFileName));
                            var fileInfo = new FileInfo(platformBankPath);
                            if (fileInfo.Exists)
                            {
                                bankRef.FileSizes.Add(new EditorBankRef.NameValuePair(bankPlatforms[i], fileInfo.Length));
                            }
                        }
                    }
                    else
                    {
                        string platformBankPath = RuntimeUtils.GetCommonPlatformPath(Path.Combine(Settings.Instance.SourceBankPath, bankFileName));
                        var fileInfo = new FileInfo(platformBankPath);
                        if (fileInfo.Exists)
                        {
                            bankRef.FileSizes.Add(new EditorBankRef.NameValuePair("", fileInfo.Length));
                        }
                    }

                    if (masterBankFileNames.Contains(bankFileInfo.Name))
                    {
                        if (!eventCache.MasterBanks.Exists(x => RuntimeUtils.GetCommonPlatformPath(bankFileName) == x.Path))
                        {
                            eventCache.MasterBanks.Add(bankRef);
                        }
                    }
                }

                // Remove any stale entries from bank, event and parameter lists
                eventCache.EditorBanks.FindAll((x) => !x.Exists).ForEach(RemoveCacheBank);
                eventCache.EditorBanks.RemoveAll((x) => !x.Exists);
                eventCache.EditorEvents.RemoveAll((x) => x.Banks.Count == 0);
                eventCache.EditorParameters.RemoveAll((x) => !x.Exists);
                eventCache.MasterBanks.RemoveAll((x) => !x.Exists);
                eventCache.StringsBanks.RemoveAll((x) => !x.Exists);
            }
            finally
            {
                // Unload the strings banks
                loadedStringsBanks.ForEach(x => x.unload());
                AssetDatabase.StopAssetEditing();
                Debug.Log("[FMOD] Cache Updated.");
            }
        }

        static void UpdateCacheBank(EditorBankRef bankRef)
        {
            // Clear out any cached events from this bank
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));

            FMOD.Studio.Bank bank; 
            bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

            if (bankRef.LoadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                EditorUtils.System.getBank(bankRef.Name, out bank);
                bank.unload();
                bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
            }

            if (bankRef.LoadResult == FMOD.RESULT.OK)
            {
                // Get studio path
                string studioPath;
                bank.getPath(out studioPath);
                bankRef.SetStudioPath(studioPath);

                // Iterate all events in the bank and cache them
                FMOD.Studio.EventDescription[] eventList;
                var result = bank.getEventList(out eventList);
                if (result == FMOD.RESULT.OK)
                {
                    foreach (var eventDesc in eventList)
                    {
                        string path;
                        result = eventDesc.getPath(out path);
                        EditorEventRef eventRef = eventCache.EditorEvents.Find((x) => x.Path == path);
                        if (eventRef == null)
                        {
                            eventRef = ScriptableObject.CreateInstance<EditorEventRef>();
                            AssetDatabase.AddObjectToAsset(eventRef, eventCache);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(eventRef));
                            eventRef.Banks = new List<EditorBankRef>();
                            eventCache.EditorEvents.Add(eventRef);
                            eventRef.Parameters = new List<EditorParamRef>();
                        }

                        eventRef.Banks.Add(bankRef);
                        Guid guid;
                        eventDesc.getID(out guid);
                        eventRef.Guid = guid;
                        eventRef.Path = eventRef.name = path;
                        eventDesc.is3D(out eventRef.Is3D);
                        eventDesc.isOneshot(out eventRef.IsOneShot);
                        eventDesc.isStream(out eventRef.IsStream);
                        eventDesc.getMaximumDistance(out eventRef.MaxDistance);
                        eventDesc.getMinimumDistance(out eventRef.MinDistance);
                        eventDesc.getLength(out eventRef.Length);
                        int paramCount = 0;
                        eventDesc.getParameterDescriptionCount(out paramCount);
                        eventRef.Parameters.ForEach((x) => x.Exists = false);
                        for (int paramIndex = 0; paramIndex < paramCount; paramIndex++)
                        {
                            FMOD.Studio.PARAMETER_DESCRIPTION param;
                            eventDesc.getParameterDescriptionByIndex(paramIndex, out param);
                            if ((param.flags & FMOD.Studio.PARAMETER_FLAGS.READONLY) != 0)
                            {
                                continue;
                            }
                            EditorParamRef paramRef = eventRef.Parameters.Find((x) => x.name == param.name);
                            if (paramRef == null)
                            {
                                paramRef = ScriptableObject.CreateInstance<EditorParamRef>();
                                AssetDatabase.AddObjectToAsset(paramRef, eventCache);
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(paramRef));
                                eventRef.Parameters.Add(paramRef);
                            }
                            paramRef.Name = param.name;
                            paramRef.name = "parameter:/" + Path.GetFileName(path) + "/" + paramRef.Name;
                            paramRef.Min = param.minimum;
                            paramRef.Max = param.maximum;
                            paramRef.Default = param.defaultvalue;
                            paramRef.Exists = true;
                        }
                        eventRef.Parameters.RemoveAll((x) => !x.Exists);
                    }
                }

                // Update global parameter list for each bank
                FMOD.Studio.PARAMETER_DESCRIPTION[] parameterDescriptions;
                result = EditorUtils.System.getParameterDescriptionList(out parameterDescriptions);
                if (result == FMOD.RESULT.OK)
                {
                    for (int i = 0; i < parameterDescriptions.Length; i++)
                    {
                        FMOD.Studio.PARAMETER_DESCRIPTION param = parameterDescriptions[i];
                        if (param.flags == FMOD.Studio.PARAMETER_FLAGS.GLOBAL)
                        {
                            EditorParamRef paramRef = eventCache.EditorParameters.Find((x) =>
                                (param.id.data1 == x.ID.data1 && param.id.data2 == x.ID.data2));
                            if (paramRef == null)
                            {
                                paramRef = ScriptableObject.CreateInstance<EditorParamRef>();
                                AssetDatabase.AddObjectToAsset(paramRef, eventCache);
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(paramRef));
                                eventCache.EditorParameters.Add(paramRef);
                                paramRef.ID = param.id;
                            }
                            paramRef.Name = param.name;
                            paramRef.name = "parameter:/" + param.name;
                            paramRef.Min = param.minimum;
                            paramRef.Max = param.maximum;
                            paramRef.Default = param.defaultvalue;
                            paramRef.Exists = true;
                        }
                    }
                }
                bank.unload();
            }
            else
            {
                Debug.LogError(string.Format("FMOD Studio: Unable to load {0}: {1}", bankRef.Name, FMOD.Error.String(bankRef.LoadResult)));
                eventCache.StringsBankWriteTime = DateTime.MinValue;
            }
        }

        static void RemoveCacheBank(EditorBankRef bankRef)
        {
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));
        }

        static EventManager()
        {
            countdownTimer = CountdownTimerReset;
            EditorApplication.update += Update;
        }

        public static void CheckValidEventRefs(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                MonoBehaviour[] allBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

                foreach (MonoBehaviour behaviour in allBehaviours)
                {
                    if (behaviour != null)
                    {
                        Type componentType = behaviour.GetType();

                        FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (FieldInfo item in fields)
                        {
                            if (HasAttribute(item, typeof(EventRefAttribute)))
                            {
                                if (item.FieldType == typeof(string))
                                {
                                    string output = item.GetValue(behaviour) as string;

                                    if (!IsValidEventRef(output))
                                    {
                                        Debug.LogWarningFormat("FMOD Studio: Unable to find FMOD Event \"{0}\" in scene \"{1}\" at path \"{2}\" \n- check the FMOD Studio event paths are set correctly in the Unity editor", output, scene.name, GetGameObjectPath(behaviour.transform));
                                    }
                                }
                                else if (typeof(IEnumerable).IsAssignableFrom(item.FieldType))
                                {
                                    foreach (var listItem in (IEnumerable)item.GetValue(behaviour))
                                    {
                                        if (listItem.GetType() == typeof(string))
                                        {
                                            string listOutput = listItem as string;
                                            if (!IsValidEventRef(listOutput))
                                            {
                                                Debug.LogWarningFormat("FMOD Studio: Unable to find FMOD Event \"{0}\" in scene \"{1}\" at path \"{2}\" \n- check the FMOD Studio event paths are set correctly in the Unity editor", listOutput, scene.name, GetGameObjectPath(behaviour.transform));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string objectPath = "/" + transform.name;
            while(transform.parent != null)
            {
                transform = transform.parent;
                objectPath = "/" + transform.name + objectPath;
            }
            return objectPath;
        }

        private static bool HasAttribute(MemberInfo provider, params Type[] attributeTypes)
        {
            Attribute[] allAttributes = Attribute.GetCustomAttributes(provider, typeof(Attribute), true);

            if (allAttributes.Length == 0)
            {
                return false;
            }
            return allAttributes.Where(a => attributeTypes.Any(x => a.GetType() == x || x.IsAssignableFrom(a.GetType()))).Any();
        }

        private static bool IsValidEventRef(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return true;
            }
            EditorEventRef eventRef = EventManager.EventFromPath(reference);
            return eventRef != null;
        }

        private const string FMODLabel = "FMOD";

        public static void CopyToStreamingAssets()
        {
            if (string.IsNullOrEmpty(Settings.Instance.SourceBankPath))
                return;

            Platform platform = Settings.Instance.CurrentEditorPlatform;

            if (platform == Settings.Instance.DefaultPlatform)
            {
                Debug.LogWarningFormat("FMOD Studio: copy banks for platform {0} : Unsupported platform", EditorUserBuildSettings.activeBuildTarget);
                return;
            }

            string bankTargetFolder =
                Settings.Instance.ImportType == ImportType.StreamingAssets
                ? Settings.Instance.TargetPath
                : Application.dataPath + (string.IsNullOrEmpty(Settings.Instance.TargetAssetPath) ? "" : '/' + Settings.Instance.TargetAssetPath);
            bankTargetFolder = RuntimeUtils.GetCommonPlatformPath(bankTargetFolder);
            Directory.CreateDirectory(bankTargetFolder);

            string bankTargetExtension =
                Settings.Instance.ImportType == ImportType.StreamingAssets
                ? ".bank"
                : ".bytes";

            string bankSourceFolder =
                Settings.Instance.HasPlatforms
                ? Settings.Instance.SourceBankPath + '/' + platform.BuildDirectory
                : Settings.Instance.SourceBankPath;
            bankSourceFolder = RuntimeUtils.GetCommonPlatformPath(bankSourceFolder);

            if (Path.GetFullPath(bankTargetFolder).TrimEnd('/').ToUpperInvariant() ==
                Path.GetFullPath(bankSourceFolder).TrimEnd('/').ToUpperInvariant())
            {
                return;
            }

            bool madeChanges = false;

            try
            {
                // Clean out any stale .bank files
                string[] existingBankFiles =
                    Directory.GetFiles(bankTargetFolder, "*" + bankTargetExtension, SearchOption.AllDirectories);

                foreach (string bankFilePath in existingBankFiles)
                {
                    string bankName = EditorBankRef.CalculateName(bankFilePath, bankTargetFolder);

                    if (!eventCache.EditorBanks.Exists(x => x.Name == bankName))
                    {
                        string assetPath = bankFilePath.Replace(Application.dataPath, AssetsFolderName);

                        if (AssetHasLabel(assetPath, FMODLabel))
                        {
                            AssetDatabase.MoveAssetToTrash(assetPath);
                            madeChanges = true;
                        }
                    }
                }

                // Copy over any files that don't match timestamp or size or don't exist
                foreach (var bankRef in eventCache.EditorBanks)
                {
                    string sourcePath = bankSourceFolder + "/" + bankRef.Name + ".bank";
                    string targetPathRelative = bankRef.Name + bankTargetExtension;
                    string targetPathFull = bankTargetFolder + "/" + targetPathRelative;

                    FileInfo sourceInfo = new FileInfo(sourcePath);
                    FileInfo targetInfo = new FileInfo(targetPathFull);

                    if (!targetInfo.Exists ||
                        sourceInfo.Length != targetInfo.Length ||
                        sourceInfo.LastWriteTime != targetInfo.LastWriteTime)
                    {
                        if (targetInfo.Exists)
                        {
                            targetInfo.IsReadOnly = false;
                        }
                        else
                        {
                            EnsureFoldersExist(targetPathRelative, bankTargetFolder);
                        }

                        File.Copy(sourcePath, targetPathFull, true);
                        targetInfo = new FileInfo(targetPathFull);
                        targetInfo.IsReadOnly = false;
                        targetInfo.LastWriteTime = sourceInfo.LastWriteTime;

                        madeChanges = true;

                        string assetString = targetPathFull.Replace(Application.dataPath, "Assets");
                        AssetDatabase.ImportAsset(assetString);
                        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetString);
                        AssetDatabase.SetLabels(obj, new string[] { FMODLabel });
                    }
                }

                RemoveEmptyFMODFolders(bankTargetFolder);
            }
            catch(Exception exception)
            {
                Debug.LogErrorFormat("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2}",
                    platform.DisplayName, bankSourceFolder, bankTargetFolder);
                Debug.LogException(exception);
                return;
            }

            if (madeChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogFormat("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2} succeeded",
                    platform.DisplayName, bankSourceFolder, bankTargetFolder);
            }
        }

        private static void EnsureFoldersExist(string filePath, string basePath)
        {
            string dataPath = Application.dataPath + "/";

            if (!basePath.StartsWith(dataPath))
            {
                throw new ArgumentException(
                    string.Format("Base path {0} is not within the Assets folder", basePath), "basePath");
            }

            int lastSlash = filePath.LastIndexOf('/');

            if (lastSlash == -1)
            {
                // No folders
                return;
            }

            string assetString = filePath.Substring(0, lastSlash);

            string[] folders = assetString.Split('/');
            string parentFolder = "Assets/" + basePath.Substring(dataPath.Length);

            for (int i = 0; i < folders.Length; ++i)
            {
                string folderPath = parentFolder + "/" + folders[i];

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(parentFolder, folders[i]);

                    var folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                    AssetDatabase.SetLabels(folder, new string[] { FMODLabel });
                }

                parentFolder = folderPath;
            }
        }

        private static void BuildTargetChanged()
        {
            RefreshBanks();
            #if UNITY_ANDROID
            Settings.Instance.AndroidUseOBB = PlayerSettings.Android.useAPKExpansionFiles;
            #endif
        }

        static void OnCacheChange()
        {
            List<string> masterBanks = new List<string>();
            List<string> banks = new List<string>();

            var settings = Settings.Instance;
            bool hasChanged = false;

            foreach (EditorBankRef bankRef in eventCache.MasterBanks)
            {
                masterBanks.Add(bankRef.Name);
            }

            if (!CompareLists(masterBanks, settings.MasterBanks))
            {
                settings.MasterBanks.Clear();
                settings.MasterBanks.AddRange(masterBanks);
                hasChanged = true;
            }

            foreach (var bankRef in eventCache.EditorBanks)
            {
                if (!eventCache.MasterBanks.Contains(bankRef) &&
                    !eventCache.StringsBanks.Contains(bankRef))
                {
                    banks.Add(bankRef.Name);
                }
            }
            banks.Sort((a, b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase));

            if (!CompareLists(banks, settings.Banks))
            {
                settings.Banks.Clear();
                settings.Banks.AddRange(banks);
                hasChanged = true;
            }

            if (hasChanged)
            {
                EditorUtility.SetDirty(settings);
            }
        }

        static bool firstUpdate = true;
        static float lastCheckTime;
        static void Update()
        {
            if (firstUpdate)
            {
                RefreshBanks();
                bool isValid;
                string validateMessage;
                EditorUtils.ValidateSource(out isValid, out validateMessage);
                if (!isValid)
                {
                    Debug.LogError("FMOD Studio: " + validateMessage);
                }
                firstUpdate = false;
                lastCheckTime = Time.realtimeSinceStartup;
            }

            if (lastCheckTime + FilePollTimeSeconds < Time.realtimeSinceStartup)
            {
                RefreshBanks();
                lastCheckTime = Time.realtimeSinceStartup;
            }
        }

        public static DateTime CacheTime
        {
            get
            {
                if (eventCache != null)
                {
                    return eventCache.StringsBankWriteTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public static List<EditorEventRef> Events
        {
            get
            {
                UpdateCache();
                return eventCache.EditorEvents;
            }
        }

        public static List<EditorBankRef> Banks
        {
            get
            {
                UpdateCache();
                return eventCache.EditorBanks;
            }
        }

        public static List<EditorParamRef> Parameters
        {
            get
            {
                UpdateCache();
                return eventCache.EditorParameters;
            }
        }

        public static List<EditorBankRef> MasterBanks
        { 
            get
            {
                UpdateCache();
                return eventCache.MasterBanks;
            }
        }

        public static bool IsLoaded
        {
            get
            {
                return Settings.Instance.SourceBankPath != null;
            }
        }

        public static bool IsValid
        {
            get
            {
                UpdateCache();
                return eventCache.StringsBankWriteTime != DateTime.MinValue;
            }
        }

        public static EditorEventRef EventFromPath(string pathOrGuid)
        {
            EditorEventRef eventRef;
            if (pathOrGuid.StartsWith("{"))
            {
                Guid guid = new Guid(pathOrGuid);
                eventRef = EventFromGUID(guid);
            }
            else
            {
                eventRef = EventFromString(pathOrGuid);
            }
            return eventRef;
        }

        public static EditorEventRef EventFromString(string path)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase));
        }

        public static EditorEventRef EventFromGUID(Guid guid)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Guid == guid);
        }

        public static EditorParamRef ParamFromPath(string name)
        {
            UpdateCache();
            return eventCache.EditorParameters.Find((x) => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public class ActiveBuildTargetListener : IActiveBuildTargetChanged
        {
            public int callbackOrder{ get { return 0; } }
            public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
            {
                BuildTargetChanged();
            }
        }

#if UNITY_2018_1_OR_NEWER
        public class PreprocessBuild : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 0; } }
            public void OnPreprocessBuild(BuildReport report)
            {
                BuildTargetChanged();
                CopyToStreamingAssets();
            }
        }

        public class PreprocessScene : IProcessSceneWithReport
        {
            public int callbackOrder { get { return 0; } }

            public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
            {
                if (report == null) return;

                CheckValidEventRefs(scene);
            }
        }
#else
        public class PreprocessBuild : IPreprocessBuild
        {
            public int callbackOrder { get { return 0; } }
            public void OnPreprocessBuild(BuildTarget target, string path)
            {
                BuildTargetChanged();
                CopyToStreamingAssets();
            }
        }

        public class PreprocessScene : IProcessScene
        {
            public int callbackOrder { get { return 0; } }

            public void OnProcessScene(UnityEngine.SceneManagement.Scene scene)
            {
                CheckValidEventRefs(scene);
            }
        }
#endif

        private static bool CompareLists(List<string> tempBanks, List<string> banks)
        {
            if (tempBanks.Count != banks.Count)
                return false;

            for (int i = 0; i < tempBanks.Count; i++)
            {
                if (tempBanks[i] != banks[i])
                    return false;
            }
            return true;
        }

        private static bool AssetHasLabel(string assetPath, string label)
        {
            AssetDatabase.ImportAsset(assetPath);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            string[] labels = AssetDatabase.GetLabels(asset);

            return labels.Contains(label);
        }

        const string AssetsFolderName = "Assets";

        public static void RemoveBanks(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                return;
            }

            string[] filePaths = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);

            foreach (string filePath in filePaths)
            {
                if (!filePath.EndsWith(".meta"))
                {
                    string assetPath = filePath.Replace(Application.dataPath, AssetsFolderName);

                    if (AssetHasLabel(assetPath, FMODLabel))
                    {
                        AssetDatabase.MoveAssetToTrash(assetPath);
                    }
                }
            }

            RemoveEmptyFMODFolders(basePath);

            if (Directory.GetFileSystemEntries(basePath).Length == 0)
            {
                string baseFolder = basePath.Replace(Application.dataPath, AssetsFolderName);
                AssetDatabase.MoveAssetToTrash(baseFolder);
            }
        }

        public static void MoveBanks(string from, string to)
        {
            if (!Directory.Exists(from))
            {
                return;
            }
            
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            string[] oldBankFiles = Directory.GetFiles(from);

            foreach (var oldBankFileName in oldBankFiles)
            {
                if (oldBankFileName.EndsWith(".meta"))
                    continue;
                string assetString = oldBankFileName.Replace(Application.dataPath, "Assets");
                AssetDatabase.ImportAsset(assetString);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetString);
                string[] labels = AssetDatabase.GetLabels(obj);
                foreach (string label in labels)
                {
                    if (label.Equals("FMOD"))
                    {
                        AssetDatabase.MoveAsset(assetString, to);
                        break;
                    }
                }
            }
            if (Directory.GetFiles(Path.GetDirectoryName(oldBankFiles[0])).Length == 0)
            {
                Directory.Delete(Path.GetDirectoryName(oldBankFiles[0]));
            }
        }

        public static void RemoveEmptyFMODFolders(string basePath)
        {
            string[] folderPaths = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories);

            // Process longest paths first so parent folders are cleared out when we get to them
            Array.Sort(folderPaths, (a, b) => b.Length.CompareTo(a.Length));

            foreach (string folderPath in folderPaths)
            {
                string assetPath = folderPath.Replace(Application.dataPath, AssetsFolderName);

                if (AssetHasLabel(assetPath, FMODLabel) && Directory.GetFileSystemEntries(folderPath).Length == 0)
                {
                    AssetDatabase.MoveAssetToTrash(assetPath);
                }
            }
        }
    }
}

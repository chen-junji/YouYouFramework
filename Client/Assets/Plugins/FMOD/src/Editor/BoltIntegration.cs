using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

#if UNITY_BOLT_EXIST
using Ludiq;
using Bolt;
#endif

namespace FMODUnity
{
    public class BoltIntegration : MonoBehaviour
    {
        [MenuItem("FMOD/Generate Bolt Unit Options")]
        public static void GenerateBoltUnitOptions()
        {
#if UNITY_BOLT_EXIST
            BuildBoltUnitOptions();
#else
            TriggerBuild();
#endif
        }

#if !UNITY_BOLT_EXIST
        [MenuItem("FMOD/Generate Bolt Unit Options", true)]
        private static bool IsBoltPresent()
        {
            Assembly ludiqCoreRuntimeAssembly = null;
            Assembly boltFlowEditorAssembly = null;

            try
            {
                ludiqCoreRuntimeAssembly = Assembly.Load("Ludiq.Core.Runtime");
                boltFlowEditorAssembly = Assembly.Load("Bolt.Flow.Editor");
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            return true;
        }

        private static void TriggerBuild()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            string previousSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (!previousSymbols.Contains("UNITY_BOLT_EXIST"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, previousSymbols + ";UNITY_BOLT_EXIST");
            }
            Settings.Instance.BoltUnitOptionsBuildPending = true;
            AssetDatabase.Refresh();
        }

#else
        [InitializeOnLoadMethod]
        private static void RegisterCompleteBuild()
        {
            EditorApplication.delayCall += CompleteBuild;
        }

        private static void CompleteBuild()
        {
            if (Settings.Instance.BoltUnitOptionsBuildPending)
            {
                Settings.Instance.BoltUnitOptionsBuildPending = false;
                BuildBoltUnitOptions();
            }
        }

        private static void BuildBoltUnitOptions()
        {
            DictionaryAsset projectSettings = AssetDatabase.LoadAssetAtPath(PathUtility.FromProject(LudiqCore.Paths.projectSettings), typeof(DictionaryAsset)) as DictionaryAsset;

            List<LooseAssemblyName> assemblyOptions = projectSettings.dictionary["assemblyOptions"] as List<LooseAssemblyName>;

            if (!assemblyOptions.Contains("FMODUnity"))
            {
                assemblyOptions.Add("FMODUnity");
            }

            if (!assemblyOptions.Contains("FMODUnityResonance"))
            {
                assemblyOptions.Add("FMODUnityResonance");
            }

            List<Type> typeOptions = projectSettings.dictionary["typeOptions"] as List<Type>;
            Assembly fmodUnityAssembly = Assembly.Load("FMODUnity");
            Assembly fmodUnityResonanceAssembly = Assembly.Load("FMODUnityResonance");

            List<Type> allTypes = new List<Type>(GetTypesForNamespace(fmodUnityAssembly, "FMOD"));
            allTypes.AddRange(GetTypesForNamespace(fmodUnityAssembly, "FMOD.Studio"));

            foreach (Type type in allTypes)
            {
                if (!typeOptions.Contains(type))
                {
                    typeOptions.Add(type);
                }
            }

            UnitBase.Build();
        }

        private static IEnumerable<Type> GetTypesForNamespace(Assembly assembly, string requestedNamespace)
        {
            return assembly.GetTypes()
                    .Where(t => string.Equals(t.Namespace, requestedNamespace, StringComparison.Ordinal));
        }
#endif
    }
}

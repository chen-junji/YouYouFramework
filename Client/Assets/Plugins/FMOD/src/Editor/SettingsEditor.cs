using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace FMODUnity
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        string[] ToggleParent = new string[] { "Disabled", "Enabled", "Development Build Only",  };

        string[] ToggleEditor = new string[] { "Enabled", "Disabled", };

        string[] FrequencyDisplay = new string[] { "Platform Default", "22050", "24000", "32000", "44100", "48000"};
        int[] FrequencyValues = new int[] { 0, 22050, 24000, 32000, 44100, 48000 };

        string[] SpeakerModeDisplay = new string[] {
            "Stereo",
            "5.1",
            "7.1" };

        FMOD.SPEAKERMODE[] SpeakerModeValues = new FMOD.SPEAKERMODE[] {
            FMOD.SPEAKERMODE.STEREO,
            FMOD.SPEAKERMODE._5POINT1,
            FMOD.SPEAKERMODE._7POINT1};

        bool hasBankSourceChanged = false;
        string targetSubFolder;
        bool focused = false;
        bool bankFoldOutState = true;

        enum SourceType : uint
        {
            Project = 0,
            Single,
            Multi
        }

        void DisplayTriStateBool(string label, Platform platform, Platform.PropertyAccessor<TriStateBool> property)
        {
            TriStateBool current = property.Get(platform);

            if (platform.Parent != null)
            {
                bool overriden = property.HasValue(platform);
                TriStateBool parent = property.Get(platform.Parent);

                string[] toggleChild = new string[ToggleParent.Length + 1];
                Array.Copy(ToggleParent, 0, toggleChild, 1, ToggleParent.Length);
                toggleChild[0] = string.Format("Inherit ({0})", ToggleParent[(int)parent]);

                int next = EditorGUILayout.Popup(label, overriden ? (int)current + 1 : 0, toggleChild);

                if (next == 0)
                {
                    property.Clear(platform);
                }
                else
                {
                    property.Set(platform, (TriStateBool)(next-1));
                }
            }
            else if (platform is PlatformPlayInEditor)
            {
                int next = EditorGUILayout.Popup(label, (current != TriStateBool.Disabled) ? 0 : 1, ToggleEditor);
                property.Set(platform, next == 0 ? TriStateBool.Enabled : TriStateBool.Disabled);
            }
            else
            {
                int next = EditorGUILayout.Popup(label, (int)current, ToggleParent);
                property.Set(platform, (TriStateBool)next);
            }
        }

        void DisplayOutputMode(string label, Platform platform)
        {
            if (platform.ValidOutputTypes != null)
            {
                string[] valuesChild = new string[platform.ValidOutputTypes.Length + 3];
                string[] valuesChildEnum = new string[platform.ValidOutputTypes.Length + 3];
                valuesChild[0] = string.Format("Auto");
                valuesChild[1] = string.Format("No Sound");
                valuesChild[2] = string.Format("Wav Writer");
                valuesChildEnum[0] = Enum.GetName(typeof(FMOD.OUTPUTTYPE), FMOD.OUTPUTTYPE.AUTODETECT);
                valuesChildEnum[1] = Enum.GetName(typeof(FMOD.OUTPUTTYPE), FMOD.OUTPUTTYPE.NOSOUND);
                valuesChildEnum[2] = Enum.GetName(typeof(FMOD.OUTPUTTYPE), FMOD.OUTPUTTYPE.WAVWRITER);
                for (int i = 0; i < platform.ValidOutputTypes.Length; i++)
                {
                    valuesChild[i + 3] = platform.ValidOutputTypes[i].displayName;
                    valuesChildEnum[i + 3] = Enum.GetName(typeof(FMOD.OUTPUTTYPE), platform.ValidOutputTypes[i].outputType);
                }
                int currentIndex = Array.IndexOf(valuesChildEnum, platform.outputType);
                if (currentIndex == -1)
                {
                    currentIndex = 0;
                    platform.outputType = Enum.GetName(typeof(FMOD.OUTPUTTYPE), FMOD.OUTPUTTYPE.AUTODETECT);
                }
                int next = EditorGUILayout.Popup(label, currentIndex, valuesChild);
                platform.outputType = valuesChildEnum[next];
            }
        }

        Dictionary<string, bool> expandThreadAffinity = new Dictionary<string, bool>();

        void DisplayThreadAffinity(string label, Platform platform)
        {
            if (platform.CoreCount > 0 && DisplayThreadAffinityFoldout(label, platform))
            {
                EditorGUI.indentLevel++;

                DisplayThreadAffinityGroups(platform);

                EditorGUI.indentLevel--;
            }
        }

        bool DisplayThreadAffinityFoldout(string label, Platform platform)
        {
            Rect headerRect = EditorGUILayout.GetControlRect();

            Rect labelRect = headerRect;
            labelRect.width = EditorGUIUtility.labelWidth;

            bool expand;

            if (!expandThreadAffinity.TryGetValue(platform.Identifier, out expand))
            {
                expand = false;
            }

            EditorGUI.BeginChangeCheck();

            expand = EditorGUI.Foldout(labelRect, expand, label);

            if (EditorGUI.EndChangeCheck())
            {
                expandThreadAffinity[platform.Identifier] = expand;
            }

            bool useDefaults = !platform.ThreadAffinitiesProperty.HasValue;

            EditorGUI.BeginChangeCheck();

            Rect toggleRect = headerRect;
            toggleRect.xMin = labelRect.xMax;

            useDefaults = GUI.Toggle(toggleRect, useDefaults, "Use Defaults");

            if (EditorGUI.EndChangeCheck())
            {
                if (useDefaults)
                {
                    platform.ThreadAffinitiesProperty.Value.Clear();
                    platform.ThreadAffinitiesProperty.HasValue = false;
                }
                else
                {
                    platform.ThreadAffinitiesProperty.Value = new List<ThreadAffinityGroup>();
                    platform.ThreadAffinitiesProperty.HasValue = true;

                    foreach (ThreadAffinityGroup group in platform.DefaultThreadAffinities)
                    {
                        platform.ThreadAffinitiesProperty.Value.Add(new ThreadAffinityGroup(group));
                    }
                }
            }

            return expand;
        }

        const int THREAD_AFFINITY_CORES_PER_ROW = 8;

        void DisplayThreadAffinityGroups(Platform platform)
        {
            GUIStyle affinityStyle = EditorStyles.miniButton;
            float affinityWidth = affinityStyle.CalcSize(new GUIContent("00")).x;

            GUIContent anyButtonContent = new GUIContent("Any");
            float anyButtonWidth = affinityStyle.CalcSize(anyButtonContent).x;

            float threadsWidth = EditorGUIUtility.labelWidth;
            float affinitiesWidth = affinityWidth * THREAD_AFFINITY_CORES_PER_ROW + anyButtonWidth;

            bool editable = platform.ThreadAffinitiesProperty.HasValue;

            if (platform.ThreadAffinities.Any())
            {
                DisplayThreadAffinitiesHeader(threadsWidth, affinitiesWidth);

                ThreadAffinityGroup groupToDelete = null;

                EditorGUI.BeginDisabledGroup(!editable);

                foreach (ThreadAffinityGroup group in platform.ThreadAffinities)
                {
                    bool delete;
                    DisplayThreadAffinityGroup(group, platform, threadsWidth, affinitiesWidth,
                        anyButtonWidth, anyButtonContent, affinityStyle, affinityWidth, out delete);

                    if (delete)
                    {
                        groupToDelete = group;
                    }
                }

                if (groupToDelete != null)
                {
                    platform.ThreadAffinitiesProperty.Value.Remove(groupToDelete);
                }

                EditorGUI.EndDisabledGroup();
            }
            else
            {
                Rect messageRect = EditorGUILayout.GetControlRect();
                messageRect.width = threadsWidth + affinitiesWidth;
                messageRect = EditorGUI.IndentedRect(messageRect);

                GUI.Label(messageRect, "List is Empty");
            }

            if (editable)
            {
                Rect addButtonRect = EditorGUILayout.GetControlRect();
                addButtonRect.width = threadsWidth + affinitiesWidth;
                addButtonRect = EditorGUI.IndentedRect(addButtonRect);

                if (GUI.Button(addButtonRect, "Add"))
                {
                    platform.ThreadAffinitiesProperty.Value.Add(new ThreadAffinityGroup());
                }
            }
        }

        void DisplayThreadAffinitiesHeader(float threadsWidth, float affinitiesWidth)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            Rect threadsRect = controlRect;
            threadsRect.width = threadsWidth;

            threadsRect = EditorGUI.IndentedRect(threadsRect);

            GUI.Label(threadsRect, "Threads");

            Rect coresRect = controlRect;
            coresRect.x = threadsRect.xMax;
            coresRect.width = affinitiesWidth;

            GUI.Label(coresRect, "Cores");
        }

        void DisplayThreadAffinityGroup(ThreadAffinityGroup group, Platform platform,
            float threadsWidth, float affinitiesWidth, float anyButtonWidth, GUIContent anyButtonContent,
            GUIStyle affinityStyle, float affinityWidth, out bool delete)
        {
            delete = false;

            GUIStyle editButtonStyle = EditorStyles.popup;

            GUIContent editButtonContent = new GUIContent("Edit");
            Rect editButtonRect = new Rect(Vector2.zero, editButtonStyle.CalcSize(editButtonContent));

            float threadsHeight = group.threads.Count * EditorGUIUtility.singleLineHeight;

            bool editable = platform.ThreadAffinitiesProperty.HasValue;

            if (editable)
            {
                 threadsHeight += EditorGUIUtility.standardVerticalSpacing + editButtonRect.height;
            }

            float affinitiesHeight =
                Mathf.Ceil(platform.CoreCount / (float)THREAD_AFFINITY_CORES_PER_ROW) * EditorGUIUtility.singleLineHeight;

            Rect controlRect = EditorGUILayout.GetControlRect(false, Math.Max(threadsHeight, affinitiesHeight));

            Rect threadsRect = controlRect;
            threadsRect.width = threadsWidth;

            threadsRect = EditorGUI.IndentedRect(threadsRect);

            GUIStyle boxStyle = EditorStyles.textArea;

            GUI.Box(threadsRect, string.Empty, boxStyle);

            Rect threadRect = threadsRect;
            threadRect.height = EditorGUIUtility.singleLineHeight;

            foreach (ThreadType thread in group.threads)
            {
                GUI.Label(threadRect, thread.DisplayName());
                threadRect.y += threadRect.height;
            }

            if (editable)
            {
                editButtonRect.y = threadsRect.yMax - editButtonRect.height - editButtonStyle.margin.bottom;
                editButtonRect.center = new Vector2(threadsRect.center.x, editButtonRect.center.y);

                if (EditorGUI.DropdownButton(editButtonRect, editButtonContent, FocusType.Passive, editButtonStyle))
                {
                    ThreadListEditor.Show(editButtonRect, group, platform.ThreadAffinities, this);
                }
            }

            Rect affinitiesRect = controlRect;
            affinitiesRect.xMin = threadsRect.xMax;
            affinitiesRect.width = affinitiesWidth;

            GUI.Box(affinitiesRect, string.Empty, boxStyle);

            Rect anyButtonRect = affinitiesRect;
            anyButtonRect.height = affinitiesHeight;
            anyButtonRect.width = anyButtonWidth;

            if (GUI.Toggle(anyButtonRect, group.affinity == ThreadAffinity.Any, anyButtonContent, affinityStyle))
            {
                group.affinity = ThreadAffinity.Any;
            }

            Rect affinityRect = affinitiesRect;
            affinityRect.x = anyButtonRect.xMax;
            affinityRect.height = EditorGUIUtility.singleLineHeight;
            affinityRect.width = affinityWidth;

            for (int i = 0; i < platform.CoreCount; ++i)
            {
                ThreadAffinity mask = (ThreadAffinity)(1U << i);

                if (GUI.Toggle(affinityRect, (group.affinity & mask) == mask, i.ToString(), affinityStyle))
                {
                    group.affinity |= mask;
                }
                else
                {
                    group.affinity &= ~mask;
                }

                if (i % THREAD_AFFINITY_CORES_PER_ROW == THREAD_AFFINITY_CORES_PER_ROW - 1)
                {
                    affinityRect.x = anyButtonRect.xMax;
                    affinityRect.y += affinityRect.height;
                }
                else
                {
                    affinityRect.x += affinityRect.width;
                }
            }

            if (editable)
            {
                GUIStyle deleteButtonStyle = GUI.skin.button;
                GUIContent deleteButtonContent = new GUIContent("Delete");

                Rect deleteButtonRect = controlRect;
                deleteButtonRect.x = affinitiesRect.xMax;
                deleteButtonRect.width = deleteButtonStyle.CalcSize(deleteButtonContent).x;

                if (GUI.Button(deleteButtonRect, deleteButtonContent, deleteButtonStyle))
                {
                    delete = true;
                }
            }
        }

        class ThreadListEditor : EditorWindow
        {
            ThreadAffinityGroup group;
            IEnumerable<ThreadAffinityGroup> groups;
            Editor parent;

            public static void Show(Rect buttonRect, ThreadAffinityGroup group, IEnumerable<ThreadAffinityGroup> groups,
                Editor parent)
            {
                ThreadListEditor editor = CreateInstance<ThreadListEditor>();
                editor.group = group;
                editor.groups = groups;
                editor.parent = parent;

                Rect rect = new Rect(GUIUtility.GUIToScreenPoint(buttonRect.position), buttonRect.size);

                editor.ShowAsDropDown(rect, CalculateSize());
            }

            private static GUIStyle FrameStyle { get { return GUI.skin.box; } }
            private static GUIStyle ThreadStyle { get { return EditorStyles.toggle; } }

            private static Vector2 CalculateSize()
            {
                Vector2 result = Vector2.zero;

                Array enumValues = Enum.GetValues(typeof(ThreadType));

                foreach (ThreadType thread in enumValues)
                {
                    Vector2 size = ThreadStyle.CalcSize(new GUIContent(thread.DisplayName()));
                    result.x = Mathf.Max(result.x, size.x);
                }

                result.y = enumValues.Length * EditorGUIUtility.singleLineHeight
                    + (enumValues.Length - 1) * EditorGUIUtility.standardVerticalSpacing;

                result.x += FrameStyle.padding.horizontal;
                result.y += FrameStyle.padding.vertical;

                return result;
            }

            private void OnGUI()
            {
                Rect frameRect = new Rect(0, 0, position.width, position.height);

                GUI.Box(frameRect, string.Empty, FrameStyle);

                Rect threadRect = FrameStyle.padding.Remove(frameRect);
                threadRect.height = EditorGUIUtility.singleLineHeight;

                foreach (ThreadType thread in Enum.GetValues(typeof(ThreadType)))
                {
                    EditorGUI.BeginChangeCheck();

                    bool include = EditorGUI.ToggleLeft(threadRect, thread.DisplayName(), group.threads.Contains(thread));

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (include)
                        {
                            // Make sure each thread is only in one group
                            foreach (ThreadAffinityGroup other in groups)
                            {
                                other.threads.Remove(thread);
                            }

                            group.threads.Add(thread);
                            group.threads.Sort();
                        }
                        else
                        {
                            group.threads.Remove(thread);
                        }

                        parent.Repaint();
                    }

                    threadRect.y = threadRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        void DisplaySampleRate(string label, Platform platform)
        {
            int currentValue = platform.SampleRate;
            int currentIndex = Array.IndexOf(FrequencyValues, currentValue);

            if (platform.Parent != null)
            {
                int parentValue = platform.Parent.SampleRate;
                int parentIndex = Array.IndexOf(FrequencyValues, parentValue);

                string[] valuesChild = new string[FrequencyDisplay.Length + 1];
                Array.Copy(FrequencyDisplay, 0, valuesChild, 1, FrequencyDisplay.Length);
                valuesChild[0] = string.Format("Inherit ({0})", FrequencyDisplay[parentIndex]);

                bool overriden = Platform.PropertyAccessors.SampleRate.HasValue(platform);

                int next = EditorGUILayout.Popup(label, overriden ? currentIndex + 1 : 0, valuesChild);
                if (next == 0)
                {
                    Platform.PropertyAccessors.SampleRate.Clear(platform);
                }
                else
                {
                    Platform.PropertyAccessors.SampleRate.Set(platform, FrequencyValues[next - 1]);
                }
            }
            else
            {
                int next = EditorGUILayout.Popup(label, currentIndex, FrequencyDisplay);
                Platform.PropertyAccessors.SampleRate.Set(platform, FrequencyValues[next]);
            }
        }

        void DisplayBuildDirectory(string label, Platform platform)
        {
            string[] buildDirectories = EditorUtils.GetBankPlatforms();

            string currentValue = platform.BuildDirectory;
            int currentIndex = Math.Max(Array.IndexOf(buildDirectories, currentValue), 0);

            if (platform.Parent != null || platform is PlatformPlayInEditor)
            {
                string[] values = new string[buildDirectories.Length + 1];
                Array.Copy(buildDirectories, 0, values, 1, buildDirectories.Length);

                if (platform is PlatformPlayInEditor)
                {
                    Settings settings = target as Settings;
                    values[0] = string.Format("Current Unity Platform ({0})", settings.CurrentEditorPlatform.BuildDirectory);
                }
                else
                {
                    values[0] = string.Format("Inherit ({0})", platform.Parent.BuildDirectory);
                }

                bool overriden = Platform.PropertyAccessors.BuildDirectory.HasValue(platform);
                int next = EditorGUILayout.Popup(label, overriden ? currentIndex + 1 : 0, values);

                if (next == 0)
                {
                    Platform.PropertyAccessors.BuildDirectory.Clear(platform);
                    Platform.PropertyAccessors.SpeakerMode.Clear(platform);
                }
                else
                {
                    Platform.PropertyAccessors.BuildDirectory.Set(platform, buildDirectories[next - 1]);
                }
            }
            else
            {
                int next = EditorGUILayout.Popup(label, currentIndex, buildDirectories);
                Platform.PropertyAccessors.BuildDirectory.Set(platform, buildDirectories[next]);
            }
        }

        void DisplaySpeakerMode(string label, Platform platform, string helpText)
        {
            FMOD.SPEAKERMODE currentValue = platform.SpeakerMode;
            int currentIndex = Math.Max(Array.IndexOf(SpeakerModeValues, currentValue), 0);

            if (platform.Parent != null || platform is PlatformPlayInEditor)
            {
                bool overriden = Platform.PropertyAccessors.SpeakerMode.HasValue(platform);

                string[] values = new string[SpeakerModeDisplay.Length + 1];
                Array.Copy(SpeakerModeDisplay, 0, values, 1, SpeakerModeDisplay.Length);

                if (platform is PlatformPlayInEditor)
                {
                    Settings settings = target as Settings;
                    FMOD.SPEAKERMODE currentPlatformValue = settings.CurrentEditorPlatform.SpeakerMode;
                    int index = Array.IndexOf(SpeakerModeValues, currentPlatformValue);
                    values[0] = string.Format("Current Unity Platform ({0})", SpeakerModeDisplay[index]);
                }
                else
                {
                    FMOD.SPEAKERMODE parentValue = platform.Parent.SpeakerMode;
                    int index = Array.IndexOf(SpeakerModeValues, parentValue);
                    values[0] = string.Format("Inherit ({0})", SpeakerModeDisplay[index]);
                }

                bool hasBuildDirectory = Platform.PropertyAccessors.BuildDirectory.HasValue(platform);

                if (!hasBuildDirectory)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                int next = EditorGUILayout.Popup(label, overriden ? currentIndex + 1 : 0, values);
                if (next == 0)
                {
                    Platform.PropertyAccessors.SpeakerMode.Clear(platform);
                }
                else
                {
                    Platform.PropertyAccessors.SpeakerMode.Set(platform, SpeakerModeValues[next - 1]);
                }

                if (hasBuildDirectory)
                {
                    EditorGUILayout.HelpBox(helpText, MessageType.Info, false);
                }
                else
                {
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                int next = EditorGUILayout.Popup(label, currentIndex, SpeakerModeDisplay);
                Platform.PropertyAccessors.SpeakerMode.Set(platform, SpeakerModeValues[next]);
                EditorGUILayout.HelpBox(helpText, MessageType.Info, false);
            }
        }

        void DisplayCallbackHandler(string label, Platform platform)
        {
            Platform.PropertyAccessor<PlatformCallbackHandler> property = Platform.PropertyAccessors.CallbackHandler;

            if (platform.Parent != null || platform is PlatformPlayInEditor)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(label);

                bool inherit = !property.HasValue(platform);

                EditorGUI.BeginChangeCheck();

                if (platform is PlatformPlayInEditor)
                {
                    inherit = GUILayout.Toggle(inherit, "Current Unity Platform", GUILayout.ExpandWidth(false));
                }
                else
                {
                    inherit = GUILayout.Toggle(inherit, "Inherit", GUILayout.ExpandWidth(false));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (inherit)
                    {
                        property.Clear(platform);
                    }
                    else
                    {
                        property.Set(platform, property.Get(platform));
                    }
                }

                EditorGUI.BeginDisabledGroup(inherit);

                PlatformCallbackHandler next = EditorGUILayout.ObjectField(
                    property.Get(platform), typeof(PlatformCallbackHandler), false) as PlatformCallbackHandler;

                if (!inherit)
                {
                    property.Set(platform, next);
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                PlatformCallbackHandler next = EditorGUILayout.ObjectField(label, property.Get(platform),
                    typeof(PlatformCallbackHandler), false) as PlatformCallbackHandler;
                property.Set(platform, next);
            }
        }

        void DisplayInt(string label, Platform platform, Platform.PropertyAccessor<int> property, int min, int max)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            int currentValue = property.Get(platform);

            if (platform.Parent != null)
            {
                bool inherit = !property.HasValue(platform);

                inherit = GUILayout.Toggle(inherit, "Inherit");

                EditorGUI.BeginDisabledGroup(inherit);
                int next = EditorGUILayout.IntSlider(currentValue, min, max);
                EditorGUI.EndDisabledGroup();

                if (inherit)
                {
                    property.Clear(platform);
                }
                else
                {
                    property.Set(platform, next);
                }
            }
            else
            {
                int next = EditorGUILayout.IntSlider(currentValue, min, max);
                property.Set(platform, next);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DisplayLiveUpdatePort(string label, Platform platform, Platform.PropertyAccessor<int> property)
        {
            EditorGUILayout.BeginHorizontal();

            int currentValue = property.Get(platform);

            if (platform.Parent != null)
            {
                EditorGUILayout.PrefixLabel(label);

                bool inherit = !property.HasValue(platform);

                inherit = GUILayout.Toggle(inherit, "Inherit");

                EditorGUI.BeginDisabledGroup(inherit);
                int next = int.Parse(EditorGUILayout.TextField("", currentValue.ToString(), GUILayout.MinWidth(50)));
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    next = 9264;
                }
                EditorGUI.EndDisabledGroup();

                if (inherit)
                {
                    property.Clear(platform);
                }
                else
                {
                    property.Set(platform, next);
                }
            }
            else
            {
                int next = int.Parse(EditorGUILayout.TextField(label, currentValue.ToString()));
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    next = 9264;
                }
                property.Set(platform, next);
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawLinks()
        {
            string color = EditorGUIUtility.isProSkin ? "#fa4d14" : "#0000FF";
            // Docs link
            UnityEditor.EditorGUILayout.BeginHorizontal();
            {
                var linkStyle = GUI.skin.button;
                linkStyle.richText = true;
                string caption = "Open FMOD Getting Started Guide";
                caption = String.Format("<color={0}>{1}</color>", color, caption);
                bool bClicked = GUILayout.Button(caption, linkStyle, GUILayout.ExpandWidth(false), GUILayout.Height(30), GUILayout.MaxWidth(300));

                var rect = GUILayoutUtility.GetLastRect();
                rect.width = linkStyle.CalcSize(new GUIContent(caption)).x;
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

                if (bClicked)
                {
                    Application.OpenURL("https://fmod.com/resources/documentation-unity?version=2.0&page=user-guide.html");
                }
            }
            GUILayout.FlexibleSpace();
            // Support Link
            {
                var linkStyle = GUI.skin.button;
                linkStyle.richText = true;
                string caption = "Open FMOD Q&A";
                caption = String.Format("<color={0}>{1}</color>", color, caption);
                bool bClicked = GUILayout.Button(caption, linkStyle, GUILayout.ExpandWidth(false), GUILayout.Height(30), GUILayout.MaxWidth(200));

                var rect = GUILayoutUtility.GetLastRect();
                rect.width = linkStyle.CalcSize(new GUIContent(caption)).x;
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

                if (bClicked)
                {
                    Application.OpenURL("https://qa.fmod.com/");
                }
            }
            UnityEditor.EditorGUILayout.EndHorizontal();

            return true;
        }

        Dictionary<string, bool> expandPlatform = new Dictionary<string, bool>();

        private void DisplayPlatform(Platform platform)
        {
            if (!platform.Active)
            {
                return;
            }

            var label = new System.Text.StringBuilder();
            label.AppendFormat("<b>{0}</b>", platform.DisplayName);

            if (!platform.IsIntrinsic && platform.Children.Count > 0)
            {
                IEnumerable<string> children = platform.Children
                    .Where(child => child.Active)
                    .Select(child => child.DisplayName);

                if (children.Any())
                {
                    label.Append(" (");
                    label.Append(string.Join(", ", children.ToArray()));
                    label.Append(")");
                }
            }
            
            EditorGUILayout.BeginHorizontal();

            bool expand = true;

            if (platform.IsIntrinsic)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.richText = true;

                EditorGUILayout.LabelField(label.ToString(), style);
            }
            else
            {
                expand = false;

                if (expandPlatform.ContainsKey(platform.Identifier))
                {
                    expand = expandPlatform[platform.Identifier];
                }

                GUIStyle style = new GUIStyle(GUI.skin.FindStyle("Foldout"));
                style.richText = true;

                expand = EditorGUILayout.Foldout(expand, new GUIContent(label.ToString()), style);

                expandPlatform[platform.Identifier] = expand;

                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                {
                    // This avoids modifying the parent platform's children list while we're iterating over it
                    pendingPlatformDelete = platform;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (expand)
            {
                Settings settings = target as Settings;

                EditorGUI.indentLevel++;

                PlatformGroup group = platform as PlatformGroup;

                if (group != null)
                {
                    group.displayName = EditorGUILayout.DelayedTextField("Name", group.displayName);
                }
                DisplayPlatformParent(platform);

                DisplayTriStateBool("Live Update", platform, Platform.PropertyAccessors.LiveUpdate);

                if (platform.IsLiveUpdateEnabled)
                {
                    DisplayLiveUpdatePort("Live Update Port", platform, Platform.PropertyAccessors.LiveUpdatePort);
                }

                DisplayTriStateBool("Debug Overlay", platform, Platform.PropertyAccessors.Overlay);
                DisplayOutputMode("Output Mode", platform);
                DisplaySampleRate("Sample Rate", platform);

                if (settings.HasPlatforms)
                {
                    bool prevChanged = GUI.changed;
                    DisplayBuildDirectory("Bank Platform", platform);
                    hasBankSourceChanged |= !prevChanged && GUI.changed;

                    string helpText = string.Format(
                        "Match the speaker mode to the setting of the platform <b>{0}</b> inside FMOD Studio",
                        platform.BuildDirectory);

                    DisplaySpeakerMode("Speaker Mode", platform, helpText);
                }
                else if (platform is PlatformDefault)
                {
                    DisplaySpeakerMode("Speaker Mode", platform,
                        "Match the speaker mode to the setting inside FMOD Studio");
                }

                DisplayCallbackHandler("Callback Handler", platform);

                if (!(platform is PlatformPlayInEditor))
                {
                    DisplayInt("Virtual Channel Count", platform, Platform.PropertyAccessors.VirtualChannelCount, 1, 2048);
                    DisplayInt("Real Channel Count", platform, Platform.PropertyAccessors.RealChannelCount, 1, 256);
                    DisplayDSPBufferSettings(platform);

                    string warning = null;

                    BuildTargetGroup buildTargetGroup =
                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                    ScriptingImplementation scriptingBackend = PlayerSettings.GetScriptingBackend(buildTargetGroup);

                    if (scriptingBackend != ScriptingImplementation.IL2CPP)
                    {
                        warning = "Only supported on the IL2CPP scripting backend";
                    }

                    DisplayPlugins("Static Plugins", platform, Platform.PropertyAccessors.StaticPlugins,
                        expandStaticPlugins, warning);
                }

                DisplayPlugins("Dynamic Plugins", platform, Platform.PropertyAccessors.Plugins, expandDynamicPlugins);

                DisplayThreadAffinity("Thread Affinity", platform);

                if (!platform.IsIntrinsic)
                {
                    foreach (Platform child in platform.Children)
                    {
                        DisplayPlatform(child);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        Dictionary<string, bool> expandDynamicPlugins = new Dictionary<string, bool>();
        Dictionary<string, bool> expandStaticPlugins = new Dictionary<string, bool>();

        private void DisplayDSPBufferSettings(Platform platform)
        {
            Platform.PropertyAccessor<int> lengthProperty = Platform.PropertyAccessors.DSPBufferLength;
            Platform.PropertyAccessor<int> countProperty = Platform.PropertyAccessors.DSPBufferCount;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("DSP Buffer Settings");

            int nextLength = 0;
            int nextCount = 0;

            if (platform.Parent != null)
            {
                bool inherit = !lengthProperty.HasValue(platform) && !countProperty.HasValue(platform);

                inherit = GUILayout.Toggle(inherit, "Inherit");

                GUILayout.Space(30);

                EditorGUI.BeginDisabledGroup(inherit);

                bool useAutoDSPBufferSettings = UsingAutoDSPBufferSettings(platform, lengthProperty, countProperty);

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (!useAutoDSPBufferSettings)
                {
                    DisplayDSPBufferFields(platform, lengthProperty, countProperty, out nextLength, out nextCount);
                }

                EditorGUI.EndDisabledGroup();
                
                if (inherit)
                {
                    lengthProperty.Clear(platform);
                    countProperty.Clear(platform);
                }
                else
                {
                    lengthProperty.Set(platform, nextLength);
                    countProperty.Set(platform, nextCount);
                }
            }
            else
            {
                bool useAutoDSPBufferSettings = UsingAutoDSPBufferSettings(platform, lengthProperty, countProperty);

                EditorGUILayout.EndHorizontal();

                if (!useAutoDSPBufferSettings)
                {
                    DisplayDSPBufferFields(platform, lengthProperty, countProperty, out nextLength, out nextCount);
                }

                lengthProperty.Set(platform, nextLength);
                countProperty.Set(platform, nextCount);
            }
        }

        private bool UsingAutoDSPBufferSettings(Platform platform, Platform.PropertyAccessor<int> lengthProperty, Platform.PropertyAccessor<int> countProperty)
        {
            bool useAutoDSPBufferSettings = lengthProperty.Get(platform) == 0 && countProperty.Get(platform) == 0;

            EditorGUI.BeginChangeCheck();
            useAutoDSPBufferSettings = GUILayout.Toggle(useAutoDSPBufferSettings, "Auto");
            if (EditorGUI.EndChangeCheck())
            {
                if (useAutoDSPBufferSettings)
                {
                    lengthProperty.Set(platform, 0);
                    countProperty.Set(platform, 0);

                }
                else
                {
                    // set a helpful default value (real default is 0 for auto behaviour)
                    lengthProperty.Set(platform, 512);
                    countProperty.Set(platform, 4);
                }
            }

            return useAutoDSPBufferSettings;
        }

        private void DisplayDSPBufferFields(Platform platform, Platform.PropertyAccessor<int> lengthProperty, Platform.PropertyAccessor<int> countProperty, out int nextLength, out int nextCount)
        {
            EditorGUI.indentLevel++;
            nextLength = Mathf.Max(EditorGUILayout.IntField("DSP Buffer Length", lengthProperty.Get(platform)), 8);
            nextCount = Mathf.Max(EditorGUILayout.IntField("DSP Buffer Count", countProperty.Get(platform)), 2);
            EditorGUI.indentLevel--;
        }

        private void DisplayPlugins(string title, Platform platform,
            Platform.PropertyAccessor<List<string>> property, Dictionary<string, bool> expandState,
            string warning = null)
        {
            List<string> plugins = property.Get(platform);

            bool expand;
            expandState.TryGetValue(platform.Identifier, out expand);

            Rect controlRect = EditorGUILayout.GetControlRect();

            Rect titleRect = controlRect;
            titleRect.width = EditorGUIUtility.labelWidth;

            GUIContent buttonContent = new GUIContent("Add Plugin");

            Rect buttonRect = controlRect;
            buttonRect.xMin = buttonRect.xMax - GUI.skin.button.CalcSize(buttonContent).x;

            string fullTitle = string.Format("{0}: {1}", title, plugins.Count);

            expand = EditorGUI.Foldout(titleRect, expand, new GUIContent(fullTitle), true);

            bool inherit = false;

            if (platform.Parent != null || platform is PlatformPlayInEditor)
            {
                inherit = !property.HasValue(platform);

                EditorGUI.BeginChangeCheck();

                Rect toggleRect = controlRect;
                toggleRect.xMin = titleRect.xMax;
                toggleRect.xMax = buttonRect.xMin;

                if (platform is PlatformPlayInEditor)
                {
                    inherit = GUI.Toggle(toggleRect, inherit, "Current Unity Platform");
                }
                else
                {
                    inherit = GUI.Toggle(toggleRect, inherit, "Inherit");
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (inherit)
                    {
                        property.Clear(platform);
                    }
                    else
                    {
                        plugins = new List<string>(property.Get(platform.Parent));
                        property.Set(platform, plugins);

                        if (plugins.Count > 0)
                        {
                            expand = true;
                        }
                    }
                }
            }

            EditorGUI.BeginDisabledGroup(inherit);

            if (GUI.Button(buttonRect, buttonContent))
            {
                plugins.Add(string.Empty);
                expand = true;
            }

            if (expand)
            {
                EditorGUI.indentLevel++;

                if (warning != null)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }

                for (int i = 0; i < plugins.Count; i++)
                {
                    bool delete;
                    plugins[i] = DrawPlugin(i, plugins[i], out delete);

                    if (delete)
                    {
                        plugins.RemoveAt(i);

                        if (plugins.Count == 0)
                        {
                            expand = false;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndDisabledGroup();

            expandState[platform.Identifier] = expand;
        }

        private string DrawPlugin(int index, string name, out bool delete)
        {
            Rect controlRect = EditorGUILayout.GetControlRect();

            GUIContent deleteText = new GUIContent("Delete");

            GUIStyle buttonStyle = GUI.skin.button;

            Rect deleteButtonRect = controlRect;
            deleteButtonRect.xMin = controlRect.xMax - buttonStyle.CalcSize(deleteText).x;

            Rect nameRect = controlRect;
            nameRect.xMax = deleteButtonRect.xMin - buttonStyle.margin.left;

            string label = string.Format("Plugin {0}:", index + 1);

            string newName = EditorGUI.TextField(nameRect, label, name);

            delete = GUI.Button(deleteButtonRect, deleteText, EditorStyles.miniButton);

            return newName;
        }

        private Platform pendingPlatformDelete;

        public override void OnInspectorGUI()
        {
            Settings settings = target as Settings;

            DrawLinks();

            EditorGUI.BeginChangeCheck();

            hasBankSourceChanged = false;
            bool hasBankTargetChanged = false;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            GUI.skin.FindStyle("HelpBox").richText = true;

            SourceType sourceType = settings.HasSourceProject ? SourceType.Project : (settings.HasPlatforms ? SourceType.Multi : SourceType.Single);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            sourceType = GUILayout.Toggle(sourceType == SourceType.Project, "Project", "Button") ? 0 : sourceType;
            sourceType = GUILayout.Toggle(sourceType == SourceType.Single, "Single Platform Build", "Button") ? SourceType.Single : sourceType;
            sourceType = GUILayout.Toggle(sourceType == SourceType.Multi, "Multiple Platform Build", "Button") ? SourceType.Multi : sourceType;
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.HelpBox(
                "<size=11>Select the way you wish to connect Unity to the FMOD Studio content:\n" +
                "<b>• Project</b>\t\tIf you have the complete FMOD Studio project avaliable\n" +
                "<b>• Single Platform</b>\tIf you have only the contents of the <i>Build</i> folder for a single platform\n" +
                "<b>• Multiple Platforms</b>\tIf you have only the contents of the <i>Build</i> folder for multiple platforms, each platform in its own sub directory\n" + 
                "</size>"
                , MessageType.Info, true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (sourceType == SourceType.Project)
            {
                EditorGUILayout.BeginHorizontal();
                string oldPath = settings.SourceProjectPath;
                EditorGUILayout.PrefixLabel("Studio Project Path", GUI.skin.textField, style);

                EditorGUI.BeginChangeCheck();
                string newPath = EditorGUILayout.TextField(GUIContent.none, settings.SourceProjectPath);
                if (EditorGUI.EndChangeCheck())
                {
                    if (newPath.EndsWith(".fspro"))
                    {
                        settings.SourceProjectPath = newPath;
                    }
                }

                if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(null);
                    EditorApplication.update += BrowseForSourceProjectPath;
                }
                EditorGUILayout.EndHorizontal();

                // Cache in settings for runtime access in play-in-editor mode
                string bankPath = EditorUtils.GetBankDirectory();
                settings.SourceBankPath = bankPath;
                settings.HasPlatforms = true;
                settings.HasSourceProject = true;

                // First time project path is set or changes, copy to streaming assets
                if (settings.SourceProjectPath != oldPath)
                {
                    hasBankSourceChanged = true;
                }
            }
            else if (sourceType == SourceType.Single || sourceType == SourceType.Multi)
            {
                EditorGUILayout.BeginHorizontal();
                string oldPath = settings.SourceBankPath;
                EditorGUILayout.PrefixLabel("Build Path", GUI.skin.textField, style);

                EditorGUI.BeginChangeCheck();
                string tempPath = EditorGUILayout.TextField(GUIContent.none, settings.SourceBankPath);
                if (EditorGUI.EndChangeCheck())
                {
                    settings.SourceBankPath = tempPath;
                }

                if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                {
                    GUI.FocusControl(null);
                    EditorApplication.update += BrowseForSourceBankPath;
                }
                EditorGUILayout.EndHorizontal();

                settings.HasPlatforms = (sourceType == SourceType.Multi);
                settings.HasSourceProject = false;

                // First time project path is set or changes, copy to streaming assets
                if (settings.SourceBankPath != oldPath)
                {
                    hasBankSourceChanged = true;
                }
            }

            bool validBanks;
            string failReason;
            EditorUtils.ValidateSource(out validBanks, out failReason);
            if (!validBanks)
            {
                failReason += "\n\nFor detailed setup instructions, please see the getting started guide linked above.";
                EditorGUILayout.HelpBox(failReason, MessageType.Error, true);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(settings);
                }
                return;
            }

            ImportType importType = (ImportType)EditorGUILayout.EnumPopup("Import Type", settings.ImportType);
            if (importType != settings.ImportType)
            {
                bool deleteBanks = EditorUtility.DisplayDialog(
                    "FMOD Bank Import Type Changed", "Do you want to delete the " + settings.ImportType.ToString() + " banks in " + settings.TargetPath,
                    "Yes", "No");

                if (deleteBanks)
                {
                    // Delete the old banks
                    EventManager.RemoveBanks(settings.TargetPath);
                }
                hasBankTargetChanged = true;
                settings.ImportType = importType;
            }

            // ----- Asset Sub Directory -------------
            {
                GUI.SetNextControlName("targetSubFolder");
                targetSubFolder = settings.ImportType == ImportType.AssetBundle
                    ? EditorGUILayout.TextField("FMOD Asset Sub Folder", string.IsNullOrEmpty(targetSubFolder) ? settings.TargetAssetPath : targetSubFolder)
                    : EditorGUILayout.TextField("FMOD Bank Sub Folder", string.IsNullOrEmpty(targetSubFolder) ? settings.TargetSubFolder : targetSubFolder);
                if (GUI.GetNameOfFocusedControl() == "targetSubFolder")
                {
                    focused = true;
                    if (Event.current.isKey)
                    {
                        switch (Event.current.keyCode)
                        {
                            case KeyCode.Return:
                            case KeyCode.KeypadEnter:
                                if (settings.TargetSubFolder != targetSubFolder)
                                {
                                    EventManager.RemoveBanks(settings.TargetPath);
                                    settings.TargetSubFolder = targetSubFolder;
                                    hasBankTargetChanged = true;
                                }
                                targetSubFolder = "";
                                break;
                        }
                    }
                }
                else if (focused)
                {
                    if (settings.TargetSubFolder != targetSubFolder)
                    {
                        EventManager.RemoveBanks(settings.TargetPath);
                        settings.TargetSubFolder = targetSubFolder;
                        hasBankTargetChanged = true;
                    }
                    targetSubFolder = "";
                }
            }

            // ----- Logging -----------------
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("<b>Logging</b>", style);
            EditorGUI.indentLevel++;
            settings.LoggingLevel = (FMOD.DEBUG_FLAGS)EditorGUILayout.EnumPopup("Logging Level", settings.LoggingLevel);
            EditorGUI.indentLevel--;

            // ----- Audio -------------------
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("<b>Audio</b>", style);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            settings.MeterChannelOrdering = (MeterChannelOrderingType)EditorGUILayout.EnumPopup("Meter Channel Ordering", settings.MeterChannelOrdering);
            if (EditorGUI.EndChangeCheck() && EventBrowser.IsOpen)
            {
                EditorWindow.GetWindow<EventBrowser>("FMOD Events", false).Repaint();
            }
            settings.StopEventsOutsideMaxDistance = EditorGUILayout.Toggle("Stop Events Outside Max Distance", settings.StopEventsOutsideMaxDistance);
            EditorGUI.indentLevel--;

            // ----- Loading -----------------
            EditorGUI.BeginDisabledGroup(settings.ImportType == ImportType.AssetBundle);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("<b>Initialization</b>", style);
            EditorGUI.indentLevel++;

            settings.EnableMemoryTracking = EditorGUILayout.Toggle("Enable Memory Tracking", settings.EnableMemoryTracking);

            settings.BankLoadType = (BankLoadType)EditorGUILayout.EnumPopup("Load Banks", settings.BankLoadType);
            switch (settings.BankLoadType)
            {
                case BankLoadType.All:
                    break;
                case BankLoadType.Specified:
                    settings.AutomaticEventLoading = false;
                    Texture upArrowTexture = EditorGUIUtility.Load("FMOD/ArrowUp.png") as Texture;
                    Texture downArrowTexture = EditorGUIUtility.Load("FMOD/ArrowDown.png") as Texture;
                    bankFoldOutState = EditorGUILayout.Foldout(bankFoldOutState, "Specified Banks", true);
                    if (bankFoldOutState)
                    {
                        for (int i = 0; i < settings.BanksToLoad.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.indentLevel++;

                            var bankName = settings.BanksToLoad[i];
                            EditorGUILayout.TextField(bankName.Replace(".bank", ""));

                            if (GUILayout.Button(upArrowTexture, GUILayout.ExpandWidth(false)))
                            {
                                if (i > 0)
                                {
                                    var temp = settings.BanksToLoad[i];
                                    settings.BanksToLoad[i] = settings.BanksToLoad[i - 1];
                                    settings.BanksToLoad[i - 1] = temp;
                                }
                                continue;
                            }
                            if (GUILayout.Button(downArrowTexture, GUILayout.ExpandWidth(false)))
                            {
                                if (i < settings.BanksToLoad.Count - 1)
                                {
                                    var temp = settings.BanksToLoad[i];
                                    settings.BanksToLoad[i] = settings.BanksToLoad[i + 1];
                                    settings.BanksToLoad[i + 1] = temp;
                                }
                                continue;
                            }

                            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                            {
                                GUI.FocusControl(null);
                                string path = EditorUtility.OpenFilePanel("Locate Bank", settings.TargetPath, "bank");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    path = RuntimeUtils.GetCommonPlatformPath(path);
                                    settings.BanksToLoad[i] = path.Replace(settings.TargetPath, "");
                                    Repaint();
                                }
                            }
                            if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                            {
                                Settings.Instance.BanksToLoad.RemoveAt(i);
                                continue;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.indentLevel--; 
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        if (GUILayout.Button("Add Bank", GUILayout.ExpandWidth(false)))
                        {
                            settings.BanksToLoad.Add("");
                        }
                        if (GUILayout.Button("Add All Banks", GUILayout.ExpandWidth(false)))
                        {
                            string sourceDir;

                            if (settings.HasSourceProject)
                            {
                                sourceDir = string.Format("{0}/{1}/", settings.SourceBankPath, settings.CurrentEditorPlatform.BuildDirectory);
                            }
                            else
                            {
                                sourceDir = settings.SourceBankPath;
                            }

                            sourceDir = RuntimeUtils.GetCommonPlatformPath(Path.GetFullPath(sourceDir));
                            var banksFound = new List<string>(Directory.GetFiles(sourceDir, "*.bank", SearchOption.AllDirectories));
                            for (int i = 0; i < banksFound.Count; i++)
                            {
                                string bankLongName = RuntimeUtils.GetCommonPlatformPath(Path.GetFullPath(banksFound[i]));
                                string bankShortName = bankLongName.Replace(sourceDir, "");
                                if (!settings.BanksToLoad.Contains(bankShortName))
                                {
                                    settings.BanksToLoad.Add(bankShortName);
                                }
                            }

                            Repaint();
                        }
                        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                        {
                            settings.BanksToLoad.Clear();
                        }
                        GUILayout.EndHorizontal();
                    }
                    break;
                case BankLoadType.None:
                    settings.AutomaticEventLoading = false;
                    break;
                default:
                    break;
            }

            EditorGUI.BeginDisabledGroup(settings.BankLoadType == BankLoadType.None);
            settings.AutomaticSampleLoading = EditorGUILayout.Toggle("Load Bank Sample Data", settings.AutomaticSampleLoading);
            EditorGUI.EndDisabledGroup();

            settings.EncryptionKey = EditorGUILayout.TextField("Bank Encryption Key", settings.EncryptionKey);

            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            // ----- PIE ----------------------------------------------
            EditorGUILayout.Separator();
            DisplayPlatform(settings.PlayInEditorPlatform);

            // ----- Default ----------------------------------------------
            EditorGUILayout.Separator();
            DisplayPlatform(settings.DefaultPlatform);

            // Top-level platforms
            EditorGUILayout.Separator();
            DisplayPlatformHeader();

            EditorGUI.indentLevel++;
            foreach (Platform platform in settings.DefaultPlatform.Children)
            {
                DisplayPlatform(platform);
            }
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);
            }

            if (hasBankSourceChanged)
            {
                EventManager.RefreshBanks();
            }
            if (hasBankTargetChanged)
            {
                EventManager.RefreshBanks();
            }
            if (pendingPlatformDelete != null)
            {
                settings.RemovePlatformProperties(pendingPlatformDelete);

                ParentCandidates.Remove(pendingPlatformDelete);

                if (!(pendingPlatformDelete is PlatformGroup))
                {
                    MissingPlatforms.Add(pendingPlatformDelete);
                    MissingPlatforms.Sort(CompareDisplayNames);
                }

                pendingPlatformDelete = null;
            }
        }

        [NonSerialized]
        private Rect AddPlatformButtonRect;

        [NonSerialized]
        private List<Platform> ParentCandidates;

        [NonSerialized]
        private List<Platform> MissingPlatforms;

        private static int CompareDisplayNames(Platform a, Platform b)
        {
            return EditorUtility.NaturalCompare(a.DisplayName, b.DisplayName);
        }

        private void BuildPlatformLists()
        {
            if (MissingPlatforms == null)
            {
                MissingPlatforms = new List<Platform>();
                ParentCandidates = new List<Platform>();

                Settings settings = target as Settings;

                settings.ForEachPlatform(platform =>
                    {
                        if (!platform.Active)
                        {
                            MissingPlatforms.Add(platform);
                        }
                        else if (!platform.IsIntrinsic)
                        {
                            ParentCandidates.Add(platform);
                        }
                    });

                MissingPlatforms.Sort(CompareDisplayNames);
                ParentCandidates.Sort(CompareDisplayNames);
            }
        }

        private void AddPlatformProperties(object data)
        {
            string identifier = data as string;

            Settings settings = target as Settings;
            Platform platform = settings.FindPlatform(identifier);

            settings.AddPlatformProperties(platform);

            MissingPlatforms.Remove(platform);

            ParentCandidates.Add(platform);
            ParentCandidates.Sort(CompareDisplayNames);
        }

        private void DisplayPlatformHeader()
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.richText = true;

            GUIStyle dropdownStyle = new GUIStyle(GUI.skin.FindStyle("dropdownButton"));
            dropdownStyle.fixedHeight = 0;

            BuildPlatformLists();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("<b>Platforms</b>", dropdownStyle, labelStyle);

            EditorGUI.BeginDisabledGroup(MissingPlatforms.Count == 0);

            bool showPlatforms = EditorGUILayout.DropdownButton(new GUIContent("Add Platform"), FocusType.Passive, dropdownStyle);

            EditorGUI.EndDisabledGroup();

            if (Event.current.type == EventType.Repaint)
            {
                AddPlatformButtonRect = GUILayoutUtility.GetLastRect();
            }

            if (GUILayout.Button(new GUIContent("Add Group")))
            {
                Settings settings = target as Settings;
                settings.AddPlatformGroup("Group");
                MissingPlatforms = null;
            }

            EditorGUILayout.EndHorizontal();

            if (showPlatforms)
            {
                GenericMenu menu = new GenericMenu();

                foreach (Platform platform in MissingPlatforms)
                {
                    menu.AddItem(new GUIContent(platform.DisplayName), false, AddPlatformProperties, platform.Identifier);
                }

                menu.DropDown(AddPlatformButtonRect);
            }
        }

        private Dictionary<Platform, Rect> PlatformParentRect = new Dictionary<Platform, Rect>();

        private void DisplayPlatformParent(Platform platform)
        {
            if (!platform.IsIntrinsic)
            {
                BuildPlatformLists();

                Settings settings = target as Settings;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("Inherit From");
                bool showList = EditorGUILayout.DropdownButton(new GUIContent(platform.Parent.DisplayName), FocusType.Passive);

                if (Event.current.type == EventType.Repaint)
                {
                    PlatformParentRect[platform] = GUILayoutUtility.GetLastRect();
                }

                if (showList)
                {
                    GenericMenu menu = new GenericMenu();
#if UNITY_2018_2_OR_NEWER
                    menu.allowDuplicateNames = true;
#endif

                    GenericMenu.MenuFunction2 setParent = (newParent) =>
                    {
                        platform.Parent = newParent as Platform;
                    };

                    Action<Platform> AddMenuItem = (candidate) =>
                    {
                        bool isCurrent = platform.Parent == candidate;
                        menu.AddItem(new GUIContent(candidate.DisplayName), isCurrent, setParent, candidate);
                    };

                    AddMenuItem(settings.DefaultPlatform);

                    bool separatorAdded = false;

                    foreach (Platform candidate in ParentCandidates)
                    {
                        if (!candidate.InheritsFrom(platform))
                        {
                            if (!separatorAdded)
                            {
                                menu.AddSeparator(string.Empty);
                                separatorAdded = true;
                            }

                            AddMenuItem(candidate);
                        }
                    }

                    menu.DropDown(PlatformParentRect[platform]);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void BrowseForSourceProjectPath()
        {
            Settings settings = target as Settings;

            string newPath = EditorUtility.OpenFilePanel("Locate Studio Project", settings.SourceProjectPath, "fspro");

            if (!string.IsNullOrEmpty(newPath))
            {
                newPath = MakePathRelative(newPath);

                if (newPath != settings.SourceProjectPath)
                {
                    settings.SourceProjectPath = newPath;
                    EventManager.RefreshBanks();
                    Repaint();
                }
            }

            EditorApplication.update -= BrowseForSourceProjectPath;
        }

        void BrowseForSourceBankPath()
        {
            Settings settings = target as Settings;

            string newPath = EditorUtility.OpenFolderPanel("Locate Build Folder", settings.SourceBankPath, null);

            if (!string.IsNullOrEmpty(newPath))
            {
                newPath = MakePathRelative(newPath);

                if (newPath != settings.SourceBankPath)
                {
                    settings.SourceBankPath = newPath;
                    EventManager.RefreshBanks();
                    Repaint();
                }
            }

            EditorApplication.update -= BrowseForSourceBankPath;
        }

        private string MakePathRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            string fullPath = Path.GetFullPath(path);
            string fullProjectPath = Path.GetFullPath(Environment.CurrentDirectory + Path.DirectorySeparatorChar);

            // If the path contains the Unity project path remove it and return the result
            if (fullPath.Contains(fullProjectPath))
            {
                fullPath = fullPath.Replace(fullProjectPath, "");
            }
            // If not, attempt to find a relative path on the same drive
            else if (Path.GetPathRoot(fullPath) == Path.GetPathRoot(fullProjectPath))
            {
                // Remove trailing slash from project path for split count simplicity
                if (fullProjectPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.CurrentCulture)) fullProjectPath = fullProjectPath.Substring(0, fullProjectPath.Length - 1);

                string[] fullPathSplit = fullPath.Split(Path.DirectorySeparatorChar);
                string[] projectPathSplit = fullProjectPath.Split(Path.DirectorySeparatorChar);
                int minNumSplits = Mathf.Min(fullPathSplit.Length, projectPathSplit.Length);
                int numCommonElements = 0;
                for (int i = 0; i < minNumSplits; i++)
                {
                    if (fullPathSplit[i] == projectPathSplit[i])
                    {
                        numCommonElements++;
                    }
                    else
                    {
                        break;
                    }
                }
                string result = "";
                int fullPathSplitLength = fullPathSplit.Length;
                for (int i = numCommonElements; i < fullPathSplitLength; i++)
                {
                    result += fullPathSplit[i];
                    if (i < fullPathSplitLength - 1)
                    {
                        result += '/';
                    }
                }

                int numAdditionalElementsInProjectPath = projectPathSplit.Length - numCommonElements;
                for (int i = 0; i < numAdditionalElementsInProjectPath; i++)
                {
                    result = "../" + result;
                }

                fullPath = result;
            }

            return fullPath.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}

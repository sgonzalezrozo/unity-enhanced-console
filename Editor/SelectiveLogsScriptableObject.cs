#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
    public class SelectiveLogsScriptableObject : ScriptableObject
    {
        [SerializeField] private System.Collections.Generic.List<SelectiveLogSection> systemDefinedSections = new();
        [SerializeField] private System.Collections.Generic.List<SelectiveLogSection> userDefinedSections = new();

        public System.Collections.Generic.IReadOnlyList<SelectiveLogSection> SystemDefinedSections => systemDefinedSections;
        public System.Collections.Generic.IReadOnlyList<SelectiveLogSection> UserDefinedSections => userDefinedSections;

        [MenuItem("Tools/Enhance Console System/Setup", false, 1)]
        public static void CreateSelectiveLogsConfig()
        {
            string resourcesPath = "Assets/Resources/Tools/EnhanceConsole";
            if (!System.IO.Directory.Exists(resourcesPath))
                System.IO.Directory.CreateDirectory(resourcesPath);

            string assetPath = System.IO.Path.Combine(resourcesPath, "SelectiveLogsConfig.asset");
            SelectiveLogsScriptableObject config = AssetDatabase.LoadAssetAtPath<SelectiveLogsScriptableObject>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SelectiveLogsScriptableObject>();
                config.InitializeDefaultSections();
                AssetDatabase.CreateAsset(config, assetPath);
            }
            else
            {
                config.InitializeDefaultSections();
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
        }

        public void InitializeDefaultSections()
        {
            if (systemDefinedSections.Count > 0)
                return;

            systemDefinedSections = new System.Collections.Generic.List<SelectiveLogSection>(SectionLogsConfig.Sections);
        }

        public void AddUserDefinedSection(string sectionName, Color color)
        {
            if (string.IsNullOrEmpty(sectionName))
                return;

            userDefinedSections.Add(new SelectiveLogSection(sectionName, color));
        }

        public void RemoveUserDefinedSection(int index)
        {
            if (index >= 0 && index < userDefinedSections.Count)
                userDefinedSections.RemoveAt(index);
        }

        public SelectiveLogSection[] GetAllSections()
        {
            System.Collections.Generic.List<SelectiveLogSection> allSections = new(systemDefinedSections);
            allSections.AddRange(userDefinedSections);
            return allSections.ToArray();
        }

        internal SelectiveLogSection GetFirstMatchingSection(LogEntry log, SLFilterMode filterMode)
        {
            if (log == null)
                return null;

            SelectiveLogSection[] allSections = GetAllSections();

            foreach (SelectiveLogSection section in allSections)
            {
                if (SectionMatches(section, log, filterMode))
                    return section;
            }

            return null;
        }

        internal System.Collections.Generic.List<SelectiveLogSection> GetAllMatchingSections(LogEntry log, SLFilterMode filterMode)
        {
            System.Collections.Generic.List<SelectiveLogSection> matches = new();

            if (log == null)
                return matches;

            SelectiveLogSection[] allSections = GetAllSections();

            foreach (SelectiveLogSection section in allSections)
            {
                if (SectionMatches(section, log, filterMode))
                    matches.Add(section);
            }

            return matches;
        }

        private static bool SectionMatches(SelectiveLogSection section, LogEntry log, SLFilterMode filterMode)
        {
            bool foundInStack = false;
            bool foundInContent = false;

            if (!string.IsNullOrEmpty(log.StackTrace))
                foundInStack = section.Matches(log.StackTrace);

            if (!string.IsNullOrEmpty(log.Message))
                foundInContent = section.Matches(log.Message);

            return filterMode switch
            {
                SLFilterMode.StackAndContent => foundInStack || foundInContent,
                SLFilterMode.OnlyStack => foundInStack,
                SLFilterMode.OnlyContent => foundInContent,
                _ => foundInStack || foundInContent
            };
        }
    }

    [System.Serializable]
    public class SelectiveLogSection
    {
        [SerializeField] public string name;
        [SerializeField] public Color color;
        [SerializeField] public bool enabled = true;
        [SerializeField] public string pattern = "";
        [SerializeField] public bool useRegex;

        public SelectiveLogSection(string name, Color color, bool enabled = true)
        {
            this.name = name;
            this.color = color;
            this.enabled = enabled;
            this.pattern = "";
            this.useRegex = false;
        }

        public bool Matches(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            string searchPattern = useRegex && !string.IsNullOrEmpty(pattern) ? pattern : name;

            if (useRegex)
            {
                try
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(text, searchPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                catch
                {
                    return text.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }

            string textNoSpaces = text.Replace(" ", "");
            string patternNoSpaces = searchPattern.Replace(" ", "");
            return textNoSpaces.IndexOf(patternNoSpaces, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
#endif

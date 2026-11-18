#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    public class SelectiveLogsScriptableObject : ScriptableObject
    {
        [SerializeField] private System.Collections.Generic.List<SelectiveLogSection> systemDefinedSections = new();
        [SerializeField] private System.Collections.Generic.List<SelectiveLogSection> userDefinedSections = new();

        public System.Collections.Generic.IReadOnlyList<SelectiveLogSection> SystemDefinedSections => systemDefinedSections;
        public System.Collections.Generic.IReadOnlyList<SelectiveLogSection> UserDefinedSections => userDefinedSections;

        private const string CONFIG_FOLDER = "Assets/EnhancedConsole";
        private const string CONFIG_PATH = CONFIG_FOLDER + "/SelectiveLogsConfig.asset";

        public static SelectiveLogsScriptableObject FindConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:SelectiveLogsScriptableObject");
            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<SelectiveLogsScriptableObject>(path);
        }

        [MenuItem("Tools/Enhanced Console/Setup", false, 1)]
        public static void CreateSelectiveLogsConfig()
        {
            if (!AssetDatabase.IsValidFolder(CONFIG_FOLDER))
                AssetDatabase.CreateFolder("Assets", "EnhancedConsole");

            SelectiveLogsScriptableObject config = FindConfig();
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SelectiveLogsScriptableObject>();
                config.InitializeDefaultSections();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
            }
            else
            {
                string currentPath = AssetDatabase.GetAssetPath(config);
                if (currentPath != CONFIG_PATH)
                {
                    string error = AssetDatabase.MoveAsset(currentPath, CONFIG_PATH);
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogWarning($"[EnhancedConsole] Could not move config: {error}");
                }

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
            SectionTextMatcher.Build(this, out System.Text.RegularExpressions.Regex regex, out string plainNeedle, out bool plainIgnoresSpaces);
            return SectionTextMatcher.Matches(text, regex, plainNeedle, plainIgnoresSpaces);
        }
    }

    /// <summary>
    /// Section text matching without per-call allocations or exceptions.
    /// Plain names match case-insensitively and ignore spaces. Regexes are parsed once by the caller.
    /// </summary>
    internal static class SectionTextMatcher
    {
        private const System.Text.RegularExpressions.RegexOptions RegexFlags =
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant;

        public static void Build(SelectiveLogSection section, out System.Text.RegularExpressions.Regex regex, out string plainNeedle, out bool plainIgnoresSpaces)
        {
            regex = null;
            string name = section.name ?? "";

            if (section.useRegex)
            {
                string pattern = string.IsNullOrEmpty(section.pattern) ? name : section.pattern;
                if (pattern.Length > 0)
                {
                    try
                    {
                        regex = new System.Text.RegularExpressions.Regex(pattern, RegexFlags);
                    }
                    catch (System.ArgumentException)
                    {
                        // Invalid regex: fall back to the section name as plain text, checked once here
                        // instead of throwing for every log on every repaint.
                    }
                }

                if (regex != null)
                {
                    plainNeedle = null;
                    plainIgnoresSpaces = false;
                    return;
                }

                plainNeedle = name.ToUpperInvariant();
                plainIgnoresSpaces = false;
                return;
            }

            plainNeedle = name.Replace(" ", "").ToUpperInvariant();
            plainIgnoresSpaces = true;
        }

        public static bool Matches(string text, System.Text.RegularExpressions.Regex regex, string plainNeedle, bool plainIgnoresSpaces)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (regex != null)
                return regex.IsMatch(text);

            // An empty name matches nothing. Before, it matched every log.
            if (string.IsNullOrEmpty(plainNeedle))
                return false;

            return plainIgnoresSpaces
                ? ContainsIgnoringSpaces(text, plainNeedle)
                : text.IndexOf(plainNeedle, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Case-insensitive search for an upper-case, space-free needle, skipping spaces in the text.
        /// Equivalent to comparing both strings with spaces removed, without allocating.
        /// </summary>
        private static bool ContainsIgnoringSpaces(string text, string upperNeedle)
        {
            char first = upperNeedle[0];
            for (int i = 0; i < text.Length; i++)
            {
                if (char.ToUpperInvariant(text[i]) != first)
                    continue;

                int t = i + 1;
                int n = 1;
                while (n < upperNeedle.Length && t < text.Length)
                {
                    char c = text[t];
                    if (c == ' ')
                    {
                        t++;
                        continue;
                    }

                    if (char.ToUpperInvariant(c) != upperNeedle[n])
                        break;

                    t++;
                    n++;
                }

                if (n == upperNeedle.Length)
                    return true;
            }

            return false;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    public class SelectiveLogsScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SelectiveLogSection> systemDefinedSections = new();
        [SerializeField] private List<SelectiveLogSection> userDefinedSections = new();

        public IReadOnlyList<SelectiveLogSection> SystemDefinedSections => systemDefinedSections;
        public IReadOnlyList<SelectiveLogSection> UserDefinedSections => userDefinedSections;

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
                config = CreateConfigAsset();
            else
                UpdateExistingConfig(config);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
        }

        private static SelectiveLogsScriptableObject CreateConfigAsset()
        {
            SelectiveLogsScriptableObject config = CreateInstance<SelectiveLogsScriptableObject>();
            config.InitializeDefaultSections();
            AssetDatabase.CreateAsset(config, CONFIG_PATH);
            return config;
        }

        // Moves the config to the default path, if it's elsewhere, and adds missing default sections.
        private static void UpdateExistingConfig(SelectiveLogsScriptableObject config)
        {
            string currentPath = AssetDatabase.GetAssetPath(config);
            string error = currentPath == CONFIG_PATH ? null : AssetDatabase.MoveAsset(currentPath, CONFIG_PATH);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"[EnhancedConsole] Could not move config: {error}");

            config.InitializeDefaultSections();
            EditorUtility.SetDirty(config);
        }

        public void InitializeDefaultSections()
        {
            if (systemDefinedSections.Count > 0)
                return;

            systemDefinedSections = new List<SelectiveLogSection>(SectionLogsConfig.SECTIONS);
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
            List<SelectiveLogSection> allSections = new(systemDefinedSections);
            allSections.AddRange(userDefinedSections);
            return allSections.ToArray();
        }
    }
}
#endif

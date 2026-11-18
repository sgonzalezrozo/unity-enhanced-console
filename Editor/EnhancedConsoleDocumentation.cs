using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Links to the Enhanced Console documentation. After the package is imported, shows a welcome dialog once
    /// per project.
    /// </summary>
    [InitializeOnLoad]
    internal static class EnhancedConsoleDocumentation
    {
        internal const string URL = "https://github.com/sgonzalezrozo/unity-enhanced-console#readme";

        private const string SHOWN_KEY_PREFIX = "com.sgonzalezrozo.enhanced-console.WelcomeShown:";

        static EnhancedConsoleDocumentation()
        {
            if (Application.isBatchMode || EditorPrefs.GetBool(ShownKey, false))
                return;

            EditorApplication.update += ShowWelcomeWhenIdle;
        }

        // One key per project, so the dialog shows again in another project.
        private static string ShownKey => SHOWN_KEY_PREFIX + Application.dataPath;

        private static void ShowWelcomeWhenIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorApplication.update -= ShowWelcomeWhenIdle;
            if (EditorPrefs.GetBool(ShownKey, false))
                return;

            EditorPrefs.SetBool(ShownKey, true);
            bool open = EditorUtility.DisplayDialog(
                "Enhanced Console is installed",
                "Open it from Window > Enhanced Console. To filter logs by section, run Tools > Enhanced Console > " +
                "Setup.\n\nThe documentation covers section filtering, the toolbar and keyboard shortcuts.",
                "Open Documentation",
                "Close");

            if (open)
                OpenDocumentation();
        }

        [MenuItem("Tools/Enhanced Console/Documentation", priority = 100)]
        private static void OpenDocumentationFromMenu()
            => OpenDocumentation();

        internal static void OpenDocumentation()
            => Application.OpenURL(URL);
    }
}

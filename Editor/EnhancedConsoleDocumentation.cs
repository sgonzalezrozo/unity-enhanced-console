using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Links to the Enhanced Console documentation.
    /// </summary>
    internal static class EnhancedConsoleDocumentation
    {
        internal const string URL = "https://github.com/sgonzalezrozo/unity-enhanced-console#readme";

        [MenuItem("Tools/Enhanced Console/Documentation", priority = 100)]
        private static void OpenDocumentationFromMenu()
            => OpenDocumentation();

        internal static void OpenDocumentation()
            => Application.OpenURL(URL);
    }
}

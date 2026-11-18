#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EnhancedConsole.Editor
{
    internal sealed class EnhancedConsoleBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (EditorPrefs.GetBool(Preferences.ConsolePreferences.CLEAR_ON_BUILD_KEY, false))
                EnhancedConsoleWindow.ClearForBuild();
        }
    }
}
#endif

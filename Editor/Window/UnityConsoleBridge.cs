#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Clears Unity's own Console. Unity has no public API for it, so this calls the internal LogEntries.Clear.
    /// </summary>
    internal static class UnityConsoleBridge
    {
        public static void Clear()
        {
            try
            {
                MethodInfo clearMethod = FindClearMethod();
                if (clearMethod != null)
                    clearMethod.Invoke(null, null);
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogWarning($"[EnhancedConsole] Failed to clear Unity console: {exception.Message}");
            }
        }

        private static MethodInfo FindClearMethod()
        {
            Type logEntries = typeof(SceneView).Assembly.GetType("UnityEditor.LogEntries");
            if (logEntries == null)
            {
                Debug.LogWarning("[EnhancedConsole] Could not find LogEntries type");
                return null;
            }

            MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            if (clearMethod == null)
                Debug.LogWarning("[EnhancedConsole] Could not find Clear method on LogEntries");

            return clearMethod;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Reflection;

namespace Tools.EnhanceConsole.Editor
{
    /// <summary>
    /// Bridge to access LogContextHelper from runtime assembly using reflection.
    /// Needed because editor assembly may not have direct reference to runtime code.
    /// </summary>
    internal static class LogContextBridge
    {
        private static Type logContextHelperType;
        private static MethodInfo initializeMethod;
        private static MethodInfo getLastContextMethod;
        private static MethodInfo clearContextMethod;

        static LogContextBridge()
        {
            try
            {
                // Find LogContextHelper type in loaded assemblies
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    logContextHelperType = assembly.GetType("LogContextHelper");
                    if (logContextHelperType != null)
                        break;
                }

                if (logContextHelperType == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Type[] types = assembly.GetTypes();
                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i].Name == "LogContextHelper")
                            {
                                logContextHelperType = types[i];
                                break;
                            }
                        }

                        if (logContextHelperType != null)
                            break;
                    }
                }

                if (logContextHelperType != null)
                {
                    initializeMethod = logContextHelperType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
                    getLastContextMethod = logContextHelperType.GetMethod("GetLastLogContextInstanceId", BindingFlags.Public | BindingFlags.Static);
                    clearContextMethod = logContextHelperType.GetMethod("ClearLogContext", BindingFlags.Public | BindingFlags.Static);
                }
            }
            catch
            {
                // Silently fail if LogContextHelper is not available
            }
        }

        public static void Initialize()
        {
            if (initializeMethod == null)
                return;

            try
            {
                initializeMethod.Invoke(null, null);
            }
            catch
            {
                // Silently fail
            }
        }

        public static int GetLastLogContextInstanceId()
        {
            if (getLastContextMethod == null)
                return 0;

            try
            {
                object result = getLastContextMethod.Invoke(null, null);
                return result != null ? (int)result : 0;
            }
            catch
            {
                return 0;
            }
        }

        public static void ClearLogContext()
        {
            if (clearContextMethod == null)
                return;

            try
            {
                clearContextMethod.Invoke(null, null);
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
#endif

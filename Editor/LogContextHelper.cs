#if UNITY_EDITOR
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
    public sealed class LogContextHandler : ILogHandler
    {
        private readonly ILogHandler defaultLogHandler;

        public LogContextHandler()
        {
            defaultLogHandler = Debug.unityLogger.logHandler;
        }

        [HideInCallstack]
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            CaptureContext(context);
            defaultLogHandler.LogFormat(logType, context, format, args);
        }

        [HideInCallstack]
        public void LogException(System.Exception exception, Object context)
        {
            CaptureContext(context);
            defaultLogHandler.LogException(exception, context);
        }

        private static void CaptureContext(Object context)
        {
            if (context == null)
                return;

            if (context is GameObject gameObject)
                LogContextHelper.SetLastLogContextInstanceId(gameObject.GetInstanceID());
            else if (context is Component component)
                LogContextHelper.SetLastLogContextInstanceId(component.gameObject.GetInstanceID());
        }
    }

    public static class LogContextHelper
    {
        private static bool isInitialized;
        private static int lastLoggedGameObjectInstanceId;

        public static void Initialize()
        {
            if (isInitialized)
                return;

            Debug.unityLogger.logHandler = new LogContextHandler();
            isInitialized = true;
        }

        internal static void SetLastLogContextInstanceId(int instanceId)
        {
            lastLoggedGameObjectInstanceId = instanceId;
        }

        public static int GetLastLogContextInstanceId()
        {
            return lastLoggedGameObjectInstanceId;
        }

        public static void ClearLogContext()
        {
            lastLoggedGameObjectInstanceId = 0;
        }
    }
}
#endif
